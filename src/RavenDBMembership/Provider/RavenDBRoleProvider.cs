using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Collections.Specialized;
using Microsoft.Practices.ServiceLocation;
using Raven.Client;
using Raven.Client.Client;
using System.IO;

namespace RavenDBMembership.Provider
{
	public class RavenDBRoleProvider : RoleProvider
	{
		private const string ProviderName = "RavenDBRole";
		private IDocumentStore documentStore;

		public IDocumentStore DocumentStore
		{
			get
			{
				if (documentStore == null)
				{
					throw new NullReferenceException("The DocumentStore is not set. Please set the DocumentStore or make sure that the Common Service Locator can find the IDocumentStore and call Initialize on this provider.");
				}
				return this.documentStore;
			}
			set { this.documentStore = value; }
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			// Try to find an IDocumentStore via Common Service Locator. 
			try
			{
				var locator = ServiceLocator.Current;
				if (locator != null)
				{
					this.DocumentStore = locator.GetInstance<IDocumentStore>();
				}
			}
			catch (NullReferenceException) // Swallow Nullreference expection that occurs when there is no current service locator.
			{
			}
			base.Initialize(name, config);
		}

		public override string ApplicationName { get; set; }

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				try
				{
					var users = session.Advanced.LuceneQuery<User>().OpenSubclause();
					foreach (var username in usernames)
					{
						users = users.WhereEquals("Username", username, true);
					}
					users = users.CloseSubclause().AndAlso().WhereEquals("ApplicationName", this.ApplicationName, true);

					var usersAsList = users.ToList();
					var roles = session.Advanced.LuceneQuery<Role>().OpenSubclause();
					foreach (var roleName in roleNames)
					{
						roles = roles.WhereEquals("Name", roleName, true);
					}
					roles = roles.CloseSubclause().AndAlso().WhereEquals("ApplicationName", this.ApplicationName);

					var roleIds = roles.Select(r => r.Id).ToList();
					foreach (var roleId in roleIds)
					{
						foreach (var user in usersAsList)
						{
							user.Roles.Add(roleId);
						}
					}
					session.SaveChanges();
				}
				catch (Exception ex)
				{
					// TODO: log exception properly
					Console.WriteLine(ex.ToString());
					throw;
				}
			}
		}

		public override void CreateRole(string roleName)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				try
				{
					var role = new Role(roleName, null);
					role.ApplicationName = this.ApplicationName;

					session.Store(role);
					session.SaveChanges();
				}
				catch (Exception ex)
				{
					// TODO: log exception properly
					Console.WriteLine(ex.ToString());
					throw;
				}
			}
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				try
				{
					var role = (from r in session.Query<Role>()
							   where r.Name == roleName && r.ApplicationName == this.ApplicationName
							   select r).SingleOrDefault();
					if (role != null)
					{
						session.Delete(role);
						session.SaveChanges();
						return true;
					}
					return false;
				}
				catch (Exception ex)
				{
					// TODO: log exception properly
					Console.WriteLine(ex.ToString());
					throw;
				}
			}
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				// Get role first
				var role = (from r in session.Query<Role>()
							where r.Name == roleName && r.ApplicationName == this.ApplicationName
							select r).SingleOrDefault();
				if (role != null)
				{
					// Find users
					var users = from u in session.Query<User>()
								where u.Roles.Contains(role.Id) && u.Username.Contains(usernameToMatch)
								select u.Username;
					return users.ToArray();
				}
				return null;
			}
		}

		public override string[] GetAllRoles()
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				var roles = from r in session.Query<Role>()
							where r.ApplicationName == this.ApplicationName
							select r.Name;
				return roles.ToArray();
			}
		}

		public override string[] GetRolesForUser(string username)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				var roleIds = (from u in session.Query<User>()
							  where u.Username == username && u.ApplicationName == this.ApplicationName
							  select u.Roles).ToArray();
				var roleNames = from r in session.Query<Role>()
								where roleIds.Contains(new List<string>() {r.Id})
								select r.Name;
				return roleNames.ToArray();
			}
		}

		public override string[] GetUsersInRole(string roleName)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				var role = (from r in session.Query<Role>()
							where r.Name == roleName && r.ApplicationName == this.ApplicationName
							select r).SingleOrDefault();
				if (role != null)
				{
					var usernames = from u in session.Query<User>()
									where u.Roles.Contains(role.Id)
									select u.Username;
					return usernames.ToArray();
				}
				return null;
			}
		}

		public override bool IsUserInRole(string username, string roleName)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				var user = session.Query<User>()
					.Where(u => u.Username == username && u.ApplicationName == this.ApplicationName)
					.SingleOrDefault();
				if (user != null)
				{
					var role = (from r in session.Query<Role>()
								where r.Name == roleName && r.ApplicationName == this.ApplicationName
								select r).SingleOrDefault();
					if (role != null)
					{
						return user.Roles.Contains(role.Id);
					}
				}
				return false;
			}
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				try
				{
					var users = session.Advanced.LuceneQuery<User>().OpenSubclause();
					foreach (var username in usernames)
					{
						users = users.WhereEquals("Username", username, true);
					}
					users = users.CloseSubclause().AndAlso().WhereEquals("ApplicationName", this.ApplicationName, true);

					var usersAsList = users.ToList();
					var roles = session.Advanced.LuceneQuery<Role>().OpenSubclause();
					foreach (var roleName in roleNames)
					{
						roles = roles.WhereEquals("Name", roleName, true);
					}
					roles = roles.CloseSubclause().AndAlso().WhereEquals("ApplicationName", this.ApplicationName);

					var roleIds = roles.Select(r => r.Id).ToList();
					foreach (var roleId in roleIds)
					{
						var usersWithRole = usersAsList.Where(u => u.Roles.Contains(roleId));
						foreach (var user in usersWithRole)
						{
							user.Roles.Remove(roleId);
						}
					}
					session.SaveChanges();
				}
				catch (Exception ex)
				{
					// TODO: log exception properly
					Console.WriteLine(ex.ToString());
					throw;
				}
			}
		}

		public override bool RoleExists(string roleName)
		{
			using (var session = this.DocumentStore.OpenSession())
			{
				return session.Query<Role>().Any(r => r.Name == roleName);
			}
		}
	}
}
