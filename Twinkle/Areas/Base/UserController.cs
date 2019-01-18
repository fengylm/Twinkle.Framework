using Microsoft.AspNetCore.Mvc;
using System;
using Twinkle.Controllers;
using Twinkle.Framework.Extensions;
using Twinkle.Framework.Security.Cryptography;
using Twinkle.Models;

namespace Twinkle.Areas.Base
{
    public class UserController : BaseController
    {
        public JsonResult GetList(ClientModel client)
        {
            return this.Paging("SELECT * FROM Sys_User", "ID", client);
        }

        public JsonResult Save(ClientModel client)
        {
            Sys_User user = client.GetEntity<Sys_User>("user");
            switch (CheckExists(user))
            {
                case 1:
                    return Json(new { status = 1, msg = "用户账号已经存在" });
                case 2:
                    return Json(new { status = 1, msg = "用户名称已经存在" });
                default:
                    if (!user.ID.HasValue)
                    {
                        user.dCreatedDate = DateTime.Now;
                        user.cNonceStr = Guid.NewGuid().ToString("N");
                        user.cPassword = DataCipher.MD5Encrypt(user.UserId + user.cNonceStr + user.cPassword);
                        user.TenantId = Auth.TenantId;
                        // user.iEnabled = 1;
                    }
                    user.InsertOrUpdate();
                    return Json(new { status = 0 });

            }
        }

        private int CheckExists(Sys_User user)
        {
            if (!user.ID.HasValue)
            {
                user.TenantId = Auth.TenantId;
            }
            Sys_User similarUser = Db.ExecuteEntity<Sys_User>("SELECT * FROM Sys_User where TenantId=@TenantId and (UserId=@UserId or cName=@cName) and ID<>isnull(@ID,-1)",user);
            if (similarUser == null)
            {
                return 0;
            }
            else
            {
                if (similarUser.UserId == user.UserId)
                {
                    return 1;
                }

                if (similarUser.cName == user.cName)
                {
                    return 2;
                }

                return 0;
            }
        }
    }
}