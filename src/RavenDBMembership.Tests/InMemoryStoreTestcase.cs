using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Raven.Client.Client;

namespace RavenDBMembership.Tests
{
	public abstract class InMemoryStoreTestcase
	{
		protected EmbeddableDocumentStore NewInMemoryStore()
		{
		    var directory = Path.Combine(Path.GetTempPath(), "DealyMVCMembership");

		    DeleteDirectory(directory);

            if (Directory.Exists(directory))
                throw new Exception("could not delete directory " + directory);

			var documentStore = new EmbeddableDocumentStore
			{
                DataDirectory = directory
				//RunInMemory = true
			};
			documentStore.Initialize();
			return documentStore;
		}

        public static void DeleteDirectory(string directory)
        {
            const int retries = 15;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (Directory.Exists(directory) == false)
                        return;

                    File.SetAttributes(directory, FileAttributes.Normal);
                    Directory.Delete(directory, true);
                    return;
                }
                catch (IOException)
                {
                    foreach (var childDir in Directory.GetDirectories(directory))
                    {
                        try
                        {
                            File.SetAttributes(childDir, FileAttributes.Normal);
                        }
                        catch (IOException)
                        {
                        }
                    }

                    Thread.Sleep(100);
                }
            }

            if (Directory.Exists(directory))
                throw new Exception("Could not delete directory " + directory);
        }
	}
}
