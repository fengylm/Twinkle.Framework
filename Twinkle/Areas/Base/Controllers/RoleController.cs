using Microsoft.AspNetCore.Mvc;
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
    }
}