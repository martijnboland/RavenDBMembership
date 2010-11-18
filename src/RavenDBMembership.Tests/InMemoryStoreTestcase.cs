using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Client;

namespace RavenDBMembership.Tests
{
	public abstract class InMemoryStoreTestcase
	{
		protected EmbeddableDocumentStore NewInMemoryStore()
		{
			var documentStore = new EmbeddableDocumentStore
			{
				RunInMemory = true
			};
			documentStore.Initialize();
			return documentStore;
		}
	}
}
