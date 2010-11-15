using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Client;

namespace RavenDBMembership.Tests
{
	public abstract class InMemoryStoreTestcase
	{
		protected EmbeddablDocumentStore NewInMemoryStore()
		{
			var documentStore = new EmbeddablDocumentStore
			{
				RunInMemory = true
			};
			documentStore.Initialize();
			return documentStore;
		}
	}
}
