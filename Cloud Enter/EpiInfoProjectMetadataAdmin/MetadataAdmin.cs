using System;
using System.Windows.Forms;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.MetadataServices.Common.MetadataBlobService;
using Epi.Cloud.MetadataServices.Common;
using EpiInfoProjectMetadataAdmin;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Epi.Cloud.EpiInfoProjectMetadataAdmin
{
    public partial class MetadataAdmin : Form
    {
        static string containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);
        MetadataBlobCRUD _metadataBlobCRUD = new MetadataBlobCRUD(containerName);
        public MetadataAdmin()
        {
            InitializeComponent();
        }

        private void comboEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetBlobList();
        }

        public void GetBlobList()
        {
            lstBlob.Items.Clear();
            var blobList = _metadataBlobCRUD.GetBlobList(Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata);
            //var blobList = _metadataBlobCRUD.GetBlobListWithDescription();

            foreach (var blob in blobList)
            {
                Dictionary<string, string> metaProp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(blob);

                lstBlob.Items.Add(string.Format("Publish Date : {0} | Project Name : {1} | Project Id : {2}", metaProp[BlobMetadataKeys.PublishDate], metaProp[BlobMetadataKeys.ProjectName], metaProp[BlobMetadataKeys.ProjectId]));
            }

        }
        private void deleteBlob_Click(object sender, EventArgs e)
        {
            if (lstBlob.Items.Count > 0)
            {
                Guid SelectedBlobName = new Guid(lstBlob.SelectedItem.ToString().Split('|')[2].Trim() != string.Empty ? lstBlob.SelectedItem.ToString().Split('|')[2].Trim().Split(':')[1].Trim() : string.Empty);
                bool IsDeleted = false;

                switch (comboEnvironment.SelectedItem.ToString())
                {
                    case "CDCDev":
                        MessageBox.Show("You have Selected CDC Dev");
                        break;
                    case "CDCQA":
                        IsDeleted = _metadataBlobCRUD.DeleteBlob(SelectedBlobName.ToString("N"));
                        GetBlobList();
                        lstBlob.Refresh();

                        if (IsDeleted == true)
                        {
                            MessageBox.Show("Blob Sucessfully Deleted");
                        }
                        else
                        {
                            MessageBox.Show("Blob failed to Delete");
                        }

                        break;
                    case "Ananth":
                        MessageBox.Show("You have Select Ananth");
                        break;
                    case "Garry":
                        MessageBox.Show("You have Select Garry");
                        break;
                }
            }
            else
            {
                MessageBox.Show("Blob is not available to Delete");
            }
        }

        private void uploadBlob_Click(object sender, EventArgs e)
        {
            switch (comboEnvironment.SelectedItem.ToString())
            {
                case "CDCDev":
                    MessageBox.Show("Blob is updated with Id : ");
                    break;
                case "CDCQA":
                    MetadataProvider metadataProvider = new MetadataProvider();
                    var metaData = metadataProvider.RetrieveProjectMetadataViaAPI(Guid.Empty).Result;
                    GetBlobList();
                    lstBlob.Refresh();
                    MessageBox.Show("Blob is updated with Name : " + metaData.Project.Name);
                    break;
                case "Ananth":
                    MessageBox.Show("You have Select Ananth");
                    break;
                case "Garry":
                    MessageBox.Show("You have Select Garry");
                    break;
            }

        }

        private void lstBlob_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBlob.SelectedIndex >= 0)
            {
                deleteBlob.Enabled = true;

                btnViewBlob.Enabled = true;
            }
            else
            {
                deleteBlob.Enabled = false;

                btnViewBlob.Enabled = false;
            }
        }

        private void btnViewBlob_Click(object sender, EventArgs e)
        {
            switch (comboEnvironment.SelectedItem.ToString())
            {
                case "CDCDev":
                    MessageBox.Show("Blob is updated with Id : ");
                    break;
                case "CDCQA":
                    ViewMetaDataResponse viewMetadataRes = new ViewMetaDataResponse();
                    viewMetadataRes.txtVewMetadata.Text = _metadataBlobCRUD.DownloadText(new Guid(lstBlob.SelectedItem.ToString().Split('|')[2].Trim().Split(':')[1].Trim()).ToString("N"));
                    viewMetadataRes.ShowDialog(Owner = ParentForm);
                    break;
                case "Ananth":
                    MessageBox.Show("You have Select Ananth");
                    break;
                case "Garry":
                    MessageBox.Show("You have Select Garry");
                    break;
            }
        }
        public List<string> GetBlobMetadataList()
        {
            var metadataView = _metadataBlobCRUD.GetBlobList(Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata);
            return metadataView;
        }
    }
}
