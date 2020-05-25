using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hcs.Extensions.OdataClient.OdataParsers
{
    public class SelectMember
    {
        public string MemberPath { get; set; }
        public bool IsComplexType { get; set; }
        public SelectMember[] Split()
        {
            var pathes = MemberPath.Split('/');
            return pathes.Take(pathes.Length - 1).Select(x => new SelectMember { MemberPath = x, IsComplexType = true })
                .Append(new SelectMember { IsComplexType = IsComplexType, MemberPath = pathes.Last() }).ToArray();
        }
        public override string ToString() => MemberPath;
    }
    public class OdataSelectExpand : SelectMember
    {
        public List<SelectMember> SelectMembers { get; set; } = new List<SelectMember>();
        public List<OdataSelectExpand> Childs { get; set; } = new List<OdataSelectExpand>();
        public override string ToString()
        {
            return MemberPath == null ? "root" : MemberPath;
        }
        public void Merge(OdataSelectExpand tree)
        {
            if (tree.MemberPath == MemberPath)
            {
                SelectMembers = SelectMembers.Concat(tree.SelectMembers).Distinct().ToList();
                foreach (var child in tree.Childs)
                {
                    var selfChild = Childs.FirstOrDefault(x => x.MemberPath == child.MemberPath);
                    if (selfChild != null)
                    {
                        selfChild.Merge(child);
                    }
                    else
                    {
                        selfChild.Childs.Add(child);
                    }
                }
            }
        }

        public OdataSelectExpand GetCleanTree()
        {

            var newTree = new OdataSelectExpand();
            OdataSelectExpand seekTree(IEnumerable<SelectMember> path)
            {
                var n = newTree;
                foreach (var p in path)
                {
                    var c = n.Childs.FirstOrDefault(x => x.MemberPath == p.MemberPath);
                    if (c == null)
                    {
                        c = new OdataSelectExpand { MemberPath = p.MemberPath, IsComplexType = true };
                        n.Childs.Add(c);
                    }
                    n = c;
                }

                return n;
            }
            void recCreate(OdataSelectExpand current, IEnumerable<SelectMember> path)
            {
                var currentPath = path;
                var n = seekTree(path);
                foreach (var m in current.SelectMembers)
                {
                    var p = m.Split();
                    var cp = path.Concat(p.Take(p.Length - 1));
                    if (!currentPath.SequenceEqual(cp))
                    {
                        currentPath = cp;
                        n = seekTree(cp);
                    }
                    var mp = p.Last();
                    if (!n.SelectMembers.Any(x => x.MemberPath == mp.MemberPath))
                    {
                        n.SelectMembers.Add(mp);
                    }
                }
                foreach (var child in current.Childs)
                {
                    recCreate(child, path.Concat(child.Split()));
                }

            }
            recCreate(this, Enumerable.Empty<SelectMember>());
            return newTree;
        }
        public IEnumerable<string> GetSelect()
        {
            return SelectMembers.Where(x => !Childs.Any(y => y.MemberPath == x.MemberPath) && !x.IsComplexType).Select(x => x.MemberPath);
        }
        public IEnumerable<string> GetExpand()
        {
            string rec(OdataSelectExpand c)
            {
                var parts = new List<string>();

                var select = c.GetSelect();

                if (select.Any())
                {
                    parts.Add($"$select={string.Join(",", select)}");
                }
                var sem = c.SelectMembers.Where(x => x.IsComplexType).Select(x => x.MemberPath);
                if (c.Childs.Any() || sem.Any())
                {
                    parts.Add($"$expand={string.Join(",", sem.Concat(c.Childs.Select(x => rec(x))))}");
                }
                var se = string.Join(";", parts);
                return $"{c.MemberPath}({se})";
            }
            return SelectMembers.Where(x => x.IsComplexType).Select(x => x.MemberPath).Concat(Childs.Select(x => rec(x)));
        }
    }
}
