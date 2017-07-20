using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;

namespace Epi.DataPersistenceServices.DocumentDB
{
	public partial class SurveyResponseCRUD
	{

		private DocumentCollection ConfigureIndexing(string collectionId)
		{
			IndexingPolicy indexingPolicy = new IndexingPolicy();
			indexingPolicy.Automatic = true;
			indexingPolicy.IndexingMode = IndexingMode.Consistent;
			indexingPolicy.ExcludedPaths = new Collection<ExcludedPath>
						{
							 new ExcludedPath
							 {
								 Path="/*"
							 }
						};
			switch (collectionId)
			{
				case "FormInfo":
					// FormInfo collection indexing here.                           
					indexingPolicy.IncludedPaths = new Collection<IncludedPath>
					   {
							  new IncludedPath
							   {
								   Path="/_ts/?",
								   Indexes=GetIndexInfo()
							   },
							  new IncludedPath
							   {
								   Path="/GlobalRecordID/?",
								   Indexes=GetIndexInfo()
							   },
							   new IncludedPath
							   {
								   Path="/FormId/?",
								   Indexes=GetIndexInfo()
							   },
							  new IncludedPath
							   {
								   Path="/FormName/?",
								   Indexes=GetIndexInfo()
							   },
							  new IncludedPath
							   {
								   Path="/RecStatus/?",
								   Indexes=GetIndexInfo()
							   },
							   new IncludedPath
							   {
								   Path="/RelateParentId/?",
								   Indexes=GetIndexInfo()
							   },
							   new IncludedPath
							   {
								   Path="/IsRelatedView/?",
								   Indexes=GetIndexInfo()
							   },
							   new IncludedPath
							   {
								   Path="/IsDraftMode/?",
								   Indexes=GetIndexInfo()
							   },

						   //Every property (also the Title)gets a hash index on strings, 
					   };


					//collectionSpec.IndexingPolicy = indexingPolicy;
					break;

				default:
					indexingPolicy.IncludedPaths = new Collection<IncludedPath>
																{
																	new IncludedPath
																	{
																		Path="/_ts/?",
																		Indexes=GetIndexInfo()
																	},
																	new IncludedPath
																	{
																		Path="/GlobalRecordID/?",
																		Indexes=GetIndexInfo()
																	},
																	new IncludedPath
																	{
																		Path="/PageId/?",
																		Indexes=new Collection<Index>
																		{
																			new RangeIndex(DataType.Number)
																		}
																} 
						   //Every property (also the Title)gets a hash index on strings, 
					   };
					break;
			}

			var collectionSpec = new DocumentCollection
			{
				Id = collectionId,
				IndexingPolicy = indexingPolicy
			};
			return collectionSpec;
		}

		private Collection<Index> GetIndexInfo()
		{
			Collection<Index> Indexes = new Collection<Index>
								   {
									   new RangeIndex(DataType.Number),
									   new HashIndex(DataType.String)
								   };
			return Indexes;
		}
	}
}
