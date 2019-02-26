using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Twinkle.Controllers;
using Twinkle.Framework.Extensions;
using Twinkle.Models;

namespace Twinkle.Areas.Base.Controllers
{
    public class RoleController : BaseController
    {
        public JsonResult GetList(ClientModel client)
        {
            return this.Paging("SELECT * FROM Sys_Role", "ID", client);
        }

        #region 分配权限Module加载
        public JsonResult GetModuleData(ClientModel clientModel)
        {
            int? nRoleID = clientModel.GetInt("ID");
            Node node = new Node
            {
                label = "系统菜单",
                expand = true,
                id = 0
            };

            List<Sys_Module> listModule = Db.ExecuteEntities<Sys_Module>(@"select T.*,ISNULL(T1.nRoleID,0) iHasRole from Sys_Module T 
                                                                               LEFT JOIN Sys_RoleForModule T1 ON T.cCode=T1.cModuleCode 
                                                                               AND T1.TenantId=@TenantId and T1.nRoleID=@nRoleID
                                                                               ORDER BY T.ID", new { TenantId = Auth.TenantId, nRoleID });

            NodeBuilder(node, listModule);

            List<Node> nodes = new List<Node>();
            nodes.Add(node);

            return Json(new
            {
                nodes,
                ids = listModule.Where(m => m.iHasRole > 0).Select(m => m.ID).ToArray()
            });


        }

        public void NodeBuilder(Node pNode, List<Sys_Module> list)
        {
            foreach (var item in list.Where(p => { return p.nPID == pNode.id; }))
            {
                Node subNode = new Node();
                subNode.label = item.cTitle;
                subNode.expand = true;
                subNode.id = item.ID;
                subNode.cCode = item.cCode;
                subNode.@checked = item.iHasRole > 0;
                if (pNode.children == null)
                {
                    pNode.children = new List<Node>();
                }
                pNode.children.Add(subNode);

                NodeBuilder(subNode,list);
            }
            if (pNode.children == null)
            {
                pNode.leaf = true;
            }
            else
            {
                pNode.@checked = false;
                pNode.leaf = false;
            }
        }
        #endregion

        public JsonResult GetColumnData(ClientModel clientModel)
        {
            int? nRoleID = clientModel.GetInt("ID");
            string cModuleCode = clientModel.GetString("cCode");

            string strSQL = @"SELECT T.ID, T.cTitle,T.cField,T.cModuleCode,ISNULL(T1.nRoleID,0) iHasRole FROM Sys_ColumnsForModule T
                              LEFT JOIN Sys_RoleForColumn T1 ON T.cModuleCode=T1.cModuleCode AND T.cField=T1.cField
                              AND T1.TenantId=@TenantId and T1.nRoleID=@nRoleID 
                              WHERE T.cModuleCode=@cModuleCode and T.iShow=1
                              ORDER BY T.nOrderID";

            dynamic data = Db.ExecuteEntities<dynamic>(strSQL, new { nRoleID, cModuleCode, TenantId = Auth.TenantId });

            Node node = new Node
            {
                label = "可授权列",
                expand = true,
                leaf = false,
                key = cModuleCode,
                id = -1,
                children = new List<Node>()
            };
            foreach (var item in data)
            {
                node.children.Add(new Node
                {
                    label = item.cTitle,
                    cField = item.cField,
                    id = Convert.ToDouble(item.ID),
                    leaf = true,
                    key = cModuleCode,
                    @checked = item.iHasRole > 0
                });
            }

            var ids = (data as List<dynamic>).Where(p => p.iHasRole == 1).Select(p => p.ID).ToArray();

            return Json(new { cols = node, ids });
        }

        public JsonResult GetButtonData(ClientModel clientModel)
        {
            int? nRoleID = clientModel.GetInt("nRoleID");
            string cModuleCode = clientModel.GetString("cModuleCode");

            string strSQL = @"SELECT T.ID, T.cButtonName cTitle,T.cButtonID cField,T.cModuleCode,ISNULL(T1.nRoleID,0) iHasRole FROM Sys_ButtonsForModule T
                              LEFT JOIN Sys_RoleForButton T1 ON T.cModuleCode=T1.cModuleCode AND T.cButtonID=T1.cButtonID
                              AND T1.TenantId=@TenantId and T1.nRoleID=@nRoleID 
                              WHERE T.cModuleCode=@cModuleCode";

            dynamic data = Db.ExecuteEntities<dynamic>(strSQL, new { nRoleID, cModuleCode, TenantId = Auth.TenantId });

            Node node = new Node
            {
                label = "可授权按钮",
                id = -1,
                expand = true,
                leaf = false,
                key = cModuleCode,
                children = new List<Node>()
            };
            foreach (var item in data)
            {
                node.children.Add(new Node
                {
                    label = item.cTitle,
                    cField = item.cField,
                    id = Convert.ToDouble(item.ID),
                    leaf = true,
                    key = cModuleCode,
                    @checked = item.iHasRole > 0
                });
            }

            var ids = (data as List<dynamic>).Where(p => p.iHasRole == 1).Select(p => p.ID).ToArray();
            return Json(new { btns = node, ids });
        }

        public JsonResult ColumnRoleSet(ClientModel clientModel)
        {
            int? nRoleID = clientModel.GetInt("roleID");
            string[] fieldCodes = clientModel.GetArray<string>("fieldArr");
            string cModuleCode = clientModel.GetString("cModuleCode");
            Db.BeginTransaction();

            try
            {
                string strSQL = "DELETE Sys_RoleForColumn WHERE TenantId=@TenantId and nRoleID=@nRoleID and cModuleCode=@cModuleCode";
                Db.ExecuteNonQuery(strSQL, new { TenantId = Auth.TenantId, nRoleID, cModuleCode });

                if (fieldCodes != null && fieldCodes.Length > 0)
                {
                    List<object> insertEntity = new List<object>();
                    foreach (var item in fieldCodes)
                    {
                        insertEntity.Add(new
                        {
                            TenantId = Auth.TenantId,
                            nRoleID,
                            cModuleCode,
                            cField = item
                        });
                    }
                    strSQL = "INSERT INTO Sys_RoleForColumn(TenantId,nRoleID,cModuleCode,cField)VALUES(@TenantId,@nRoleID,@cModuleCode,@cField)";
                    Db.ExecuteNonQuery(strSQL, insertEntity);
                }

                Db.Commit();

                return Json(new
                {
                    status = 0
                });
            }
            catch (Exception ex)
            {
                Db.Rollback();
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }

        }

        public JsonResult ButtonRoleSet(ClientModel clientModel)
        {
            int? nRoleID = clientModel.GetInt("roleID");
            string[] fieldCodes = clientModel.GetArray<string>("fieldArr");
            string cModuleCode = clientModel.GetString("cModuleCode");
            Db.BeginTransaction();

            try
            {
                string strSQL = "DELETE Sys_RoleForButton WHERE TenantId=@TenantId and nRoleID=@nRoleID and cModuleCode=@cModuleCode";
                Db.ExecuteNonQuery(strSQL, new { TenantId = Auth.TenantId, nRoleID, cModuleCode });

                if (fieldCodes != null && fieldCodes.Length > 0)
                {
                    List<object> insertEntity = new List<object>();
                    foreach (var item in fieldCodes)
                    {
                        insertEntity.Add(new
                        {
                            TenantId = Auth.TenantId,
                            nRoleID,
                            cModuleCode,
                            cField = item
                        });
                    }
                    strSQL = "INSERT INTO Sys_RoleForButton(TenantId,nRoleID,cModuleCode,cButtonID)VALUES(@TenantId,@nRoleID,@cModuleCode,@cField)";
                    Db.ExecuteNonQuery(strSQL, insertEntity);
                }

                Db.Commit();

                return Json(new
                {
                    status = 0
                });
            }
            catch (Exception ex)
            {
                Db.Rollback();
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }

        }

        public JsonResult RoleSet(ClientModel clientModel)
        {
            int? status = clientModel.GetInt("status");
            int? nRoleID = clientModel.GetInt("roleID");
            Db.BeginTransaction();
            try
            {
                string strSQL = "DELETE Sys_RoleForModule WHERE TenantId=@TenantId and nRoleID=@nRoleID;";

                strSQL += "DELETE Sys_RoleForColumn WHERE TenantId=@TenantId and nRoleID=@nRoleID;";

                strSQL += "DELETE Sys_RoleForButton WHERE TenantId=@TenantId and nRoleID=@nRoleID;";
                Db.ExecuteNonQuery(strSQL, new { nRoleID, TenantId = Auth.TenantId });

                if (status == 1)
                {
                    strSQL = @"INSERT INTO Sys_RoleForModule(TenantId,nRoleID,cModuleCode)
                           SELECT @TenantId,@nRoleID,cCode from Sys_Module;";

                    strSQL += @"INSERT INTO Sys_RoleForColumn(TenantId,nRoleID,cModuleCode,cField)
                           SELECT @TenantId,@nRoleID,cModuleCode,cField FROM Sys_ColumnsForModule; ";

                    strSQL += @"INSERT INTO Sys_RoleForButton(TenantId,nRoleID,cModuleCode,cButtonID)
                           SELECT @TenantId,@nRoleID,cModuleCode,cButtonID FROM Sys_ButtonsForModule; ";

                    Db.ExecuteNonQuery(strSQL, new { nRoleID, TenantId = Auth.TenantId });
                }

                Db.Commit();

                return Json(new
                {
                    status = 0
                });
            }
            catch (Exception ex)
            {
                Db.Rollback();
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }

        }


        public JsonResult Save(ClientModel client)
        {
            Sys_Role role = client.GetEntity<Sys_Role>("role");
            switch (CheckExists(role))
            {
                case 1:
                    return Json(new { status = 1, msg = "编码已经存在" });
                case 2:
                    return Json(new { status = 1, msg = "名称已经存在" });
                default:
                    if (!role.ID.HasValue)
                    {
                        role.TenantId = Auth.TenantId;
                        role.iStatus = 1;
                    }
                    role.InsertOrUpdate();
                    return Json(new { status = 0 });

            }
        }

        public JsonResult Delete(ClientModel client)
        {
            int[] ids = client.GetArray<int>("delIds");
            Db.BeginTransaction();
            foreach (var id in ids)
            {
                Db.ExecuteNonQuery("DELETE SYS_ROLE WHERE ID=@ID AND TenantId=@TenantId", new { ID = id, Auth.TenantId });
            }
            Db.Commit();
            return Json(new { status = 0 });
        }

        public JsonResult AlterStatus(ClientModel client)
        {
            int? iStatus = client.GetInt("iStatus");
            int? ID = client.GetInt("ID");
            Db.ExecuteNonQuery("UPDATE SYS_ROLE SET iStatus=@iStatus WHERE ID=@ID AND TenantId=@TenantId", new { ID, iStatus, Auth.TenantId });
            return Json(new { status = 0 });
        }

        private int CheckExists(Sys_Role role)
        {
            if (!role.ID.HasValue)
            {
                role.TenantId = Auth.TenantId;
            }
            Sys_Role similarRole = Db.ExecuteEntity<Sys_Role>("SELECT * FROM Sys_Role where TenantId=@TenantId and (cCode=@cCode or cTitle=@cTitle) and ID<>isnull(@ID,-1)", role);
            if (similarRole == null)
            {
                return 0;
            }
            else
            {
                if (similarRole.cCode == role.cCode)
                {
                    return 1;
                }

                if (similarRole.cTitle == role.cTitle)
                {
                    return 2;
                }

                return 0;
            }
        }

        public JsonResult ModuleRoleSet(ClientModel clientModel)
        {
            int? nRoleID = clientModel.GetInt("roleID");
            string[] moduleCodes = clientModel.GetArray<string>("moduleCodeArr");
            Db.BeginTransaction();

            try
            {
                string strSQL = "DELETE Sys_RoleForModule WHERE TenantId=@nCompanyID and nRoleID=@nRoleID";
                Db.ExecuteNonQuery(strSQL, new { nCompanyID = Auth.TenantId, nRoleID });
                if (moduleCodes != null && moduleCodes.Length > 0)
                {
                    List<object> insertEntity = new List<object>();
                    foreach (var item in moduleCodes)
                    {
                        insertEntity.Add(new
                        {
                            nCompanyID = Auth.TenantId,
                            nRoleID,
                            cModuleCode = item
                        });
                    }
                    strSQL = "INSERT INTO Sys_RoleForModule(TenantId,nRoleID,cModuleCode)VALUES(@nCompanyID,@nRoleID,@cModuleCode)";
                    Db.ExecuteNonQuery(strSQL, insertEntity);

                    strSQL = @"INSERT INTO Sys_RoleForModule(TenantId,nRoleID,cModuleCode)
                       SELECT DISTINCT @nCompanyID,@nRoleID,cCode FROM Sys_Module WHERE  ID in (SELECT nPID From Sys_Module where cCode in @cModuleCode)";
                    Db.ExecuteNonQuery(strSQL, new { nCompanyID = Auth.TenantId, nRoleID, cModuleCode = moduleCodes });
                }

                Db.Commit();

                return Json(new
                {
                    status = 0
                });
            }
            catch (Exception ex)
            {
                Db.Rollback();
                return Json(new
                {
                    status = 1,
                    msg = ex.Message
                });
            }
            //return Json(new { status=0});
        }

        #region 穿梭框
        public JsonResult UserGetData()
        {
            List<UserGet> list = new List<UserGet>();
            List<Sys_User> column = Db.ExecuteEntities<Sys_User>(@"SELECT UserID,cName FROM Sys_User WHERE iStatus=1 AND TenantId=@TenantId ", new { TenantId = Auth.TenantId });
            foreach (var item in column)
            {
                UserGet users = new UserGet();
                users.key = item.UserId;
                users.label = item.cName;
                users.disabled = false;
                list.Add(users);
            }
            return Json(list);
        }

        public JsonResult GetRoleUserList(ClientModel client)
        {
            int? nRoleID = client.GetInt("nRoleID");
            dynamic roleUserList = Db.ExecuteEntities<dynamic>(@"SELECT UserId FROM Sys_UserInRole WHERE TenantId=@TenantId and nRoleID=@nRoleID ", new { TenantId = Auth.TenantId, nRoleID });
            List<string> returns = new List<string>();
            foreach (var item in roleUserList)
            {
                returns.Add(item.UserId);
            }
            return Json(returns);
        }

        public JsonResult SaveRoleUserList(ClientModel client)
        {
            int? nRoleID = client.GetInt("nRoleID");
            string[] keys = client.GetArray<string>("targetKeys");
            try
            {
                Db.BeginTransaction();
                Db.ExecuteNonQuery("delete Sys_UserInRole where TenantId=@TenantId and nRoleID=@nRoleID", new { TenantId = Auth.TenantId, nRoleID });

                List<object> paramsIn = new List<object>();
                foreach (var key in keys)
                {
                    paramsIn.Add(new { TenantId = Auth.TenantId, UserId = key, nRoleID });
                }

                Db.ExecuteNonQuery(@"INSERT INTO Sys_UserInRole (TenantId,UserId,nRoleID)
                                 VALUES(@TenantId,@UserId,@nRoleID)", paramsIn);

                Db.Commit();

                return Json(new
                {
                    status = 0
                });
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

        public class UserGet
        {
            public string key { get; set; }
            public string label { get; set; }
            public bool? disabled { get; set; }
        }
        #endregion
    }
}