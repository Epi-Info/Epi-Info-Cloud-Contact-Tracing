using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.CosmosDB.FormSettings;
using Epi.FormMetadata.DataStructures;
using Microsoft.Azure.Documents.Client;

namespace Epi.DataPersistenceServices.CosmosDB
{
    public partial class CosmosDBCRUD
    {
        private const string FormSettingsCollectionName = "FormSettings";

        public List<FormSettingsProperties> GetFormSettingsPropertiesList(IEnumerable<string> formIds)
        {
            List<FormSettingsProperties> formSettingsPropertiesList = new List<FormSettingsProperties>();
            foreach (var formId in formIds)
            {
                formSettingsPropertiesList.Add(GetFormSettingsProperties(formId));
            }
            return formSettingsPropertiesList;
        }

        public FormSettingsProperties GetFormSettingsProperties(string formId)
        {
            var formSettingsResource = ReadFormSettingsResource(formId, ifNoneCreateDefault: true);
            return formSettingsResource.FormSettingsProperties;
        }

        public List<ResponseGridColumnSettings> GetResponseGridColumns(string formId)
        {
            FormSettingsResource formSettingsResource = ReadFormSettingsResource(formId, ifNoneCreateDefault: true);
            var formSettingsProperties = formSettingsResource.FormSettingsProperties;
            var responseDisplaySettingsList = formSettingsProperties.ToResponseDisplaySettingsList();
            return responseDisplaySettingsList;
        }

        public FormSettingsResource ReadFormSettingsResource(string formId, bool ifNoneCreateDefault = false)
        {
            try
            {
                var formSettingsCollectionUri = GetCollectionUri(FormSettingsCollectionName);
                var collectionAlias = FormSettingsCollectionName;

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = Client.CreateDocumentQuery(formSettingsCollectionUri,
                    SELECT
                    + AssembleSelect(collectionAlias, "*")
                    + FROM + collectionAlias
                    + WHERE
                    + AssembleExpressions(collectionAlias, Expression("id", EQ, formId))
                    , queryOptions);
                var formSettingsResource = (FormSettingsResource)query.AsEnumerable().FirstOrDefault();
                if (formSettingsResource == null && ifNoneCreateDefault)
                {
                    formSettingsResource = SetDefaultFormSettingsProperties(formId);
                }
                return formSettingsResource;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        private FormSettingsResource SetDefaultFormSettingsProperties(string formId)
        {
            var responseGridColumnNames = GetFieldDigests(formId)
                .Where(f => !FieldDigest.NonDataFieldTypes.Any(t => t == f.FieldType))
                .Take(5)
                .Select(f => f.TrueCaseFieldName)
                .ToList();

            FormSettingsResource formSettingsResource = new FormSettingsResource
            {
                Id = formId,
                FormSettingsProperties = new FormSettingsProperties
                {
                    FormId = formId,
                    FormName = GetFormDigest(formId).FormName,
                    ColumnNames = responseGridColumnNames
                }
            };

            UpdateFormSettingsResource(formSettingsResource);
            return formSettingsResource;
        }

        public void UpdateFormSettings(Epi.Common.Core.DataStructures.FormSettings formSettings)
        {
            var formSettingsResource = ReadFormSettingsResource(formSettings.FormId, ifNoneCreateDefault: true);
            var formSettingsProperties = formSettingsResource.FormSettingsProperties;
            formSettingsProperties = formSettings.ToFormSettingsProperties(formSettingsProperties);
            formSettingsResource.FormSettingsProperties = formSettingsProperties;
            UpdateFormSettingsResource(formSettingsResource);
        }

        public void UpdateFormSettings(IEnumerable<Epi.Common.Core.DataStructures.FormSettings> formSettingsList)
        {
            foreach (var formSettings in formSettingsList)
            {
                UpdateFormSettings(formSettings);
            }
        }



        public void UpdateResponseGridColumnNames(string formId, List<ResponseGridColumnSettings> responseDisplaySettings)
        {
            var formSettingsResource = ReadFormSettingsResource(formId, ifNoneCreateDefault: true);
            var responseGridColumnNames = responseDisplaySettings.ToResponseDisplaySettingsList();
            formSettingsResource.FormSettingsProperties.ColumnNames = responseGridColumnNames;
            UpdateFormSettingsResource(formSettingsResource);
        }

        private FormSettingsResource UpdateFormSettingsResource(FormSettingsResource responseDisplaySettingsResource)
        {
            var formSettingsCollectionUri = GetCollectionUri(FormSettingsCollectionName);

            var result = ExecuteWithFollowOnAction(() => Client.UpsertDocumentAsync(formSettingsCollectionUri, responseDisplaySettingsResource));

            return responseDisplaySettingsResource;
        }
    }
}


