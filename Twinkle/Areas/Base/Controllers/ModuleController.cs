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
            return Paging(sql, "nOrderID", client, new { ID });
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

            List<Sys_Module> listModule = Db.ExecuteEntities<Sys_Module>(@"SELECT * FROM Sys_Module where nPID =0 order by nOrderID ");

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

        #region 菜单模块排序
        public JsonResult SortModule(ClientModel clientModel)
        {
            //不支持夸父节点调整 所以 current和target一定拥有相同的父节点
            Sys_Module current = clientModel.GetEntity<Sys_Module>("current");
            Sys_Module target = clientModel.GetEntity<Sys_Module>("target");

            double nPID = current.nPID.Value;

            double nTID = target.ID.Value;

            string type = clientModel.GetString("type");




            string sortSQL = string.Empty;

            if (type == "after")// 只有排序到最后一个时才会是after
            {
                sortSQL = @"BEGIN TRAN
                                DECLARE @sortTable TABLE(nOrderID INT IDENTITY,ID int)
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_Module Where nPID=@nPID AND  ID<>@ID ORDER BY ISNULL(nOrderID,999999)
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_Module Where nPID=@nPID AND ID=@ID ORDER BY ISNULL(nOrderID,999999)
                                
                                UPDATE Sys_Module SET nOrderID=(SELECT nOrderID FROM @sortTable T WHERE T.ID=Sys_Module.ID)
                                WHERE nPID=@nPID
                            COMMIT";
                Db.ExecuteNonQuery(sortSQL, new { nPID, current.ID });
            }
            else
            {
                sortSQL = @"BEGIN TRAN
                                DECLARE @sortTable TABLE(nOrderID INT IDENTITY,ID int)
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_Module Where nPID=@nPID AND  nOrderID<(SELECT nOrderID FROM Sys_Module WHERE ID=@nTID) AND ID<>@ID ORDER BY nOrderID
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_Module Where nPID=@nPID AND ID=@ID ORDER BY ISNULL(nOrderID,999999)
                                 INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_Module Where nPID=@nPID AND  nOrderID>=(SELECT nOrderID FROM Sys_Module WHERE ID=@nTID) AND ID<>@ID ORDER BY nOrderID
                                
                                UPDATE Sys_Module SET nOrderID=(SELECT nOrderID FROM @sortTable T WHERE T.ID=Sys_Module.ID)
                                WHERE nPID=@nPID
                            COMMIT";
                Db.ExecuteNonQuery(sortSQL, new { nPID, current.ID, nTID });
            }

            return GetParent();
        }
        #endregion

        #region 列布局操作
        #region 获取列配置名称
        public JsonResult GetCustomColumnModuleCode()
        {
            string strSQL = "SELECT distinct cModuleCode value,cModuleName text FROM Sys_ColumnsForModule where cModuleCode=cModuleName";
            return Json(Db.ExecuteEntities<dynamic>(strSQL));

        }
        #endregion
        #region 保存列配置信息
        public JsonResult SaveColumnConfig(ClientModel clientModel)
        {
            Sys_ColumnsForModule module = clientModel.GetEntity<Sys_ColumnsForModule>("columnModel");
            try
            {
                if (module.InsertOrUpdate() > 0)
                {
                    return Json(new
                    {
                        status = 0
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 2
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }
        }
        #endregion
        #region 加载列数据
        public JsonResult LoadConfig(ClientModel clientModel)
        {
            string cModuleCode = clientModel.GetString("cModuleCode");
            int? iShow = clientModel.GetInt("iShow");
            string strSQL = "SELECT * FROM Sys_ColumnsForModule WHERE cModuleCode=@cModuleCode and (iShow=@iShow or iShow=1)";
            return this.Paging(strSQL, "nOrderID",clientModel, new
            {
                cModuleCode = cModuleCode,
                iShow = iShow
            });
        }
        #endregion
        #region 删除列数据
        public JsonResult DeleteColumnConfig(ClientModel clientModel)
        {
            int? ID = clientModel.GetInt("ID");
            string strSQL = "delete Sys_ColumnsForModule where ID=@ID";
            try
            {
                if (Db.ExecuteNonQuery(strSQL, new { ID }) > 0)
                {
                    return Json(new
                    {
                        status = 0
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 2
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }
        }
        #endregion
        #region 列排序
        public JsonResult SortLayoutConfig(ClientModel clientModel)
        {
            //不支持夸父节点调整 所以 current和target一定拥有相同的父节点
            Sys_ColumnsForModule current = clientModel.GetEntity<Sys_ColumnsForModule>("current");
            Sys_ColumnsForModule target = clientModel.GetEntity<Sys_ColumnsForModule>("target");

            string cModuleCode = current.cModuleCode;

            double TID = target.ID.Value;

            string type = clientModel.GetString("type");




            string sortSQL = string.Empty;

            if (type == "after")// 只有排序到最后一个时才会是after
            {
                sortSQL = @"BEGIN TRAN
                                DECLARE @sortTable TABLE(nOrderID INT IDENTITY,ID int)
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_ColumnsForModule Where cModuleCode=@cModuleCode AND  ID<>@ID ORDER BY ISNULL(nOrderID,999999)
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_ColumnsForModule Where cModuleCode=@cModuleCode AND ID=@ID ORDER BY ISNULL(nOrderID,999999)
                                
                                UPDATE Sys_ColumnsForModule SET nOrderID=(SELECT nOrderID FROM @sortTable T WHERE T.ID=Sys_ColumnsForModule.ID)
                                WHERE cModuleCode=@cModuleCode
                            COMMIT";
                Db.ExecuteNonQuery(sortSQL, new { cModuleCode, current.ID });
            }
            else
            {
                sortSQL = @"BEGIN TRAN
                                DECLARE @sortTable TABLE(nOrderID INT IDENTITY,ID int)
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_ColumnsForModule Where cModuleCode=@cModuleCode AND  nOrderID<(SELECT nOrderID FROM Sys_ColumnsForModule WHERE ID=@TID) AND ID<>@ID ORDER BY nOrderID
                                INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_ColumnsForModule Where cModuleCode=@cModuleCode AND ID=@ID ORDER BY ISNULL(nOrderID,999999)
                                 INSERT INTO @sortTable(ID)
                                SELECT ID FROM Sys_ColumnsForModule Where cModuleCode=@cModuleCode AND  nOrderID>=(SELECT nOrderID FROM Sys_ColumnsForModule WHERE ID=@TID) AND ID<>@ID ORDER BY nOrderID
                                
                                UPDATE Sys_ColumnsForModule SET nOrderID=(SELECT nOrderID FROM @sortTable T WHERE T.ID=Sys_ColumnsForModule.ID)
                                WHERE cModuleCode=@cModuleCode
                            COMMIT";
                Db.ExecuteNonQuery(sortSQL, new { cModuleCode, current.ID, TID });
            }

            return Json(new { status = 0 });
        }
        #endregion
        #region 获取数据库表信息
        public JsonResult GetAppTable()
        {
            string strSQL = "SELECT DISTINCT [表名] value,[表名] label FROM V_Sys_GetTablesInfo";
            return Json(Db.ExecuteEntities<dynamic>(strSQL));
        }
        #endregion
        #region 根据数据库表信息创建初始化数据
        public JsonResult InitData(ClientModel clientModel)
        {
            string code = clientModel.GetString("code");
            string name = clientModel.GetString("name");
            string table = clientModel.GetString("table");

            string strSQL = "DELETE Sys_ColumnsForModule WHERE cModuleCode=@cModuleCode;";
            strSQL += " INSERT INTO Sys_ColumnsForModule(cModuleCode,cModuleName,cField,cTitle,cDataType,nOrderID,nWidth,cAlign,iShow)";
            strSQL += " SELECT @cModuleCode,@cModuleName,[列名称],[列备注],[列归类],ROW_NUMBER() OVER(ORDER BY (SELECT 0)),120,'center',1 FROM V_Sys_GetTablesInfo where [表名]=@table";

            try
            {
                if (Db.ExecuteNonQuery(strSQL, new { cModuleCode = code, cModuleName = name, table }) > 0)
                {
                    return Json(new
                    {
                        status = 0
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 2
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }
        }
        #endregion
        #endregion

        #region 按钮注册操作
        #region 加载注册按钮信息
        public JsonResult LoadButtonRegist(ClientModel clientModel)
        {
            string cModuleCode = clientModel.GetString("cModuleCode");
            string strSQL = "SELECT * FROM Sys_ButtonsForModule WHERE cModuleCode=@cModuleCode";
            return this.Paging(strSQL, "ID",clientModel, new
            {
                cModuleCode = cModuleCode
            });
        }
        #endregion

        #region 删除注册按钮
        public JsonResult DeleteButtonRegist(ClientModel clientModel)
        {
            int? ID = clientModel.GetInt("ID");
            string strSQL = "delete Sys_ButtonsForModule where ID=@ID";
            try
            {
                if (Db.ExecuteNonQuery(strSQL, new { ID }) > 0)
                {
                    return Json(new
                    {
                        status = 0
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 2
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }
        }
        #endregion

        #region 保存注册按钮
        public JsonResult SaveButtonRegist(ClientModel clientModel)
        {
            Sys_ButtonsForModule module = clientModel.GetEntity<Sys_ButtonsForModule>("buttonModel");
            try
            {
                if (module.InsertOrUpdate() > 0)
                {
                    return Json(new
                    {
                        status = 0
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 2
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }
            //return Json(new { status=0});
        }
        #endregion
        #endregion

        //public class Node
        //{
        //    public string label { get; set; }
        //    public Boolean? expand { get; set; }
        //    public List<Node> children { get; set; }
        //    public double? id { get; set; }


        //}
    }
}
