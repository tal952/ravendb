﻿using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Connection.Async;
using Raven.Studio.Infrastructure;

namespace Raven.Studio.Models
{
	public class DatabaseCollectionsModel : ViewModel
	{
		private string initialSelectedDatabaseName;
		public BindableCollection<CollectionModel> Collections { get; set; }
		public Observable<CollectionModel> SelectedCollection { get; set; }

		public DatabaseCollectionsModel()
		{
			ModelUrl = "/collections";
			Collections = new BindableCollection<CollectionModel>(new PrimaryKeyComparer<CollectionModel>(model => model.Name));
			SelectedCollection = new Observable<CollectionModel>();
			SelectedCollection.PropertyChanged += (sender, args) =>
			                                      	{
			                                      		var urlParser = new UrlParser(UrlUtil.Url);
			                                      		var name = SelectedCollection.Value.Name;
														if (urlParser.GetQueryParam("name") != name)
														{
															urlParser.SetQueryParam("name", name);
															urlParser.NavigateTo();
														}
			                                      	};
		}

		public override void LoadModelParameters(string parameters)
		{
			var urlParser = new UrlParser(parameters);
			initialSelectedDatabaseName = urlParser.GetQueryParam("name");
		}

		protected override Task LoadedTimerTickedAsync()
		{
			return DatabaseCommands.GetTermsCount("Raven/DocumentsByEntityName", "Tag", "", 100)
				.ContinueOnSuccess(Update);
		}

		private void Update(NameAndCount[] collections)
		{
			var collectionModels = collections.OrderByDescending(x => x.Count).Select(col => new CollectionModel(DatabaseCommands)
			{
				Name = col.Name,
				Count = col.Count
			}).ToArray();
			Collections.Match(collectionModels);

			if (initialSelectedDatabaseName != null)
			{
				SelectedCollection.Value = collectionModels.Where(x => x.Name == initialSelectedDatabaseName).FirstOrDefault();
				initialSelectedDatabaseName = null;
			}
			if (SelectedCollection.Value == null)
				SelectedCollection.Value = collectionModels.FirstOrDefault();
		}
	}
}