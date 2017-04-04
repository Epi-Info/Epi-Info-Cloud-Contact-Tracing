using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Exception;
namespace Epi.Web.WCF.SurveyService
{
    [ServiceContract]
    public interface IEWECloudManagerService
    {
        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        PublishResponse PublishSurvey(PublishRequest pRequestMessage);  

        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        OrganizationResponse GetOrganization(OrganizationRequest pRequest);

        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        OrganizationResponse GetOrganizationInfo(OrganizationRequest pRequest);
        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        OrganizationResponse GetOrganizationNames(OrganizationRequest pRequest);
        
        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        OrganizationResponse GetOrganizationByKey(OrganizationRequest pRequest);
        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        SurveyInfoResponse SetSurveyInfo(SurveyInfoRequest pRequest);           

        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        bool IsValidOrgKey(SurveyInfoRequest pRequest);

        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        PublishResponse RePublishSurvey(PublishRequest pRequestMessage);   


        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        UserAuthenticationResponse GetUser(UserAuthenticationRequest request);

        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        bool PingManagerService();   
       

        //Publish MetaData To Cloud
        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        PublishResponse MetaDataToCloud(PublishRequest pRequest);


        [OperationContract]
        [FaultContract(typeof(CustomFaultException))]
        PublishResponse RePublishMetaDataToCloud(PublishRequest pRequestMessage);
        
    }
}
