using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Twinkle.Controllers;
using Twinkle.Framework.Extensions;
using Twinkle.Models;

namespace Twinkle.Areas.Base.Controllers
{
    public class ModuleController : BaseController
    {
        public JsonResult GetList(ClientModel client)
        {
            int? ID = client.GetInt("ID") ?? 0;
            string sql = @"SELECT * FROM Sys_Module where 1=1 and nPID=@ID";
            return Paging(sql, "ID", client, new { ID });
        }

        public JsonResult GetPCode(ClientModel client)
        {
            int? ID = client.GetInt("ID") ?? 0;
            string strSQL = "SELECT 0 value,'系统模块' label union SELECT ID value,cTitle label FROM Sys_Module WHERE ISNULL(nPID,0)=0";
            return Json(new
            {
                status = 0,
                data = Db.ExecuteEntities<dynamic>(strSQL, new { ID })
            });
        }

        public JsonResult GetParent()
        {
            Node node = new Node
            {
                label = "系统模块",
                expand = true,
                id = 0
            };

            List<Sys_Module> listModule = Db.ExecuteEntities<Sys_Module>(@"SELECT * FROM Sys_Module where nPID =0 order by ID ");

            NodeBuilder(node, listModule);

            List<Node> list = new List<Node>();
            list.Add(node);

            return Json(list);
        }
        public void NodeBuilder(Node pNode, List<Sys_Module> list)
        {
            foreach (var item in list.Where(p => { return p.nPID == pNode.id; }))
            {
                Node subNode = new Node();
                subNode.label = item.cTitle + $"({item.cCode})";
                subNode.expand = true;
                subNode.id = item.ID;
                if (pNode.children == null)
                {
                    pNode.children = new List<Node>();
                }
                pNode.children.Add(subNode);

                NodeBuilder(subNode, list);
            }
        }

        public JsonResult Save(ClientModel client)
        {
            Sys_Module md = client.GetEntity<Sys_Module>("md");
            switch (CheckExists(md))
            {
                case 1:
                    return Json(new { status = 1, msg = "编码已经存在" });
                case 2:
                    return Json(new { status = 1, msg = "路由已经存在" });
                default:
                    if (!md.ID.HasValue)
                    {
                        md.iStatus = 1;
                    }
                    md.InsertOrUpdate();
                    return Json(new { status = 0 });

            }
        }

        public JsonResult Delete(ClientModel client)
        {
            int[] ids = client.GetArray<int>("delIds");
            Db.BeginTransaction();
            foreach (var id in ids)
            {
                Db.ExecuteNonQuery("DELETE Sys_Module WHERE ID=@ID", new { ID = id });
            }
            Db.Commit();
            return Json(new { status = 0 });
        }

        public JsonResult AlterStatus(ClientModel client)
        {
            int? iStatus = client.GetInt("iStatus");
            int? ID = client.GetInt("ID");
            Db.ExecuteNonQuery("UPDATE Sys_Module SET iStatus=@iStatus WHERE ID=@ID", new { ID, iStatus });
            return Json(new { status = 0 });
        }

        private int CheckExists(Sys_Module md)
        {
            Sys_Module similarMd = Db.ExecuteEntity<Sys_Module>("SELECT * FROM Sys_Module where  (cCode=@cCode or cRoute=@cRoute) and ID<>isnull(@ID,-1)", md);
            if (similarMd == null)
            {
                return 0;
            }
            else
            {
                if (similarMd.cCode == similarMd.cCode)
                {
                    return 1;
                }

                if (similarMd.cRoute == similarMd.cRoute)
                {
                    return 2;
                }

                return 0;
            }
        }

        public class Node
        {
            public string label { get; set; }
            public Boolean? expand { get; set; }
            public List<Node> children { get; set; }
            public double? id { get; set; }
            

        }
    }
}
