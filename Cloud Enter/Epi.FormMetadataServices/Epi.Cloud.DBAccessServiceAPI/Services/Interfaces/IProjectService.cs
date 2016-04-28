
using Epi.Cloud.MetadataServices.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epi.Cloud.DBAccessService.Services.Interfaces
{
    public interface IProjectService
    {
        CDTResponse GetProject(CDTUser User);//
    }
}