using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Raven.Client.Client;
using RavenDBMembership.Provider;
using System.Web.Security;
using Raven.Client;
using System.Threading;

namespace RavenDBMembership.Tests
{
	public class UserTests : InMemoryStoreTestcase
	{
		[Fact]
		public void RunRavenInMemory()
		{
			using (var store = NewInMemoryStore())
			{
				Assert.NotNull(store);
			}
		}

        [Fact]
        public void StoreUserShouldCreateId()
        {
            var newUser = new User { Username = "martijn", FullName = "Martijn Boland" };
            var newUserIdPrefix = newUser.Id;

            using (var store = NewInMemoryStore())
            {
                using (var session = store.OpenSession())
                {
                    session.Store(newUser);
                    session.SaveChanges();
                }
            }

            Assert.Equal(newUserIdPrefix + "1", newUser.Id);
        }

        [Fact]
        public void CreateNewMembershipUserShouldCreateUserDocument()
        {
            using (var store = NewInMemoryStore())
            {
                var provider = new RavenDBMembershipProvider();
                provider.DocumentStore = store;
                MembershipCreateStatus status;
                var membershipUser = provider.CreateUser("martijn", "1234ABCD", "martijn@boland.org", null, null, true, null, out status);
                Assert.Equal(MembershipCreateStatus.Success, status);
                Assert.NotNull(membershipUser);
                Assert.NotNull(membershipUser.ProviderUserKey);
                Assert.Equal("martijn", membershipUser.UserName);
            }
        }
        [Fact(Skip="Not supported")]
        public void CreateNewMembershipUserShouldFailIfUsernameAlreadyUsed()
        {
            using (var store = NewInMemoryStore())
            {
                var provider = new RavenDBMembershipProvider();
                provider.DocumentStore = store;
                MembershipCreateStatus status;
                var membershipUser = provider.CreateUser("martijn", "1234ABCD", "martijn@boland.org", null, null, true,
                                                         null, out status);

                Assert.Throws<MembershipCreateUserException>(delegate
                {
                    provider.CreateUser("martijn", "1234ABCD", "martijn@boland.org", null, null, true, null, out status);
                });

                Assert.Equal(MembershipCreateStatus.DuplicateUserName, status);
            }
        }

	    [Fact]
		public void ChangePassword()
		{
			using (var store = NewInMemoryStore())
			{
				// Arrange
				var provider = new RavenDBMembershipProvider();
				provider.DocumentStore = store;
				MembershipCreateStatus status;
				var membershipUser = provider.CreateUser("martijn", "1234ABCD", "martijn@boland.org", null, null, true, null, out status);

				// Act
				provider.ChangePassword("martijn", "1234ABCD", "DCBA4321");
				Thread.Sleep(500);

				// Assert
				Assert.True(provider.ValidateUser("martijn", "DCBA4321"));
			}
		}

		[Fact]
		public void DeleteUser()
		{
			using (var store = NewInMemoryStore())
			{
				// Arrange
				var provider = new RavenDBMembershipProvider();
				provider.DocumentStore = store;
				MembershipCreateStatus status;
				var membershipUser = provider.CreateUser("martijn", "1234ABCD", "martijn@boland.org", null, null, true, null, out status);

				// Act
				provider.DeleteUser("martijn", true);

				// Assert
				Thread.Sleep(500);
				using (var session = store.OpenSession())
				{
					Assert.Equal(0, session.Query<User>().Count());
				}
			}
		}

		[Fact]
		public void GetAllUsersShouldReturnAllUsers()
		{
			using (var store = NewInMemoryStore())
			{
				// Arrange
				CreateUsersInDocumentStore(store, 5);
				var provider = new RavenDBMembershipProvider();
				provider.DocumentStore = store;
				
				// Act
				Thread.Sleep(500);
				int totalRecords;
				var membershipUsers = provider.GetAllUsers(0, 10, out totalRecords);

				// Assert
				Assert.Equal(5, totalRecords);				
				Assert.Equal(5, membershipUsers.Count);				
			}
		}

		[Fact]
		public void FindUsersByUsernamePart()
		{
			using (var store = NewInMemoryStore())
			{
				// Arrange
				CreateUsersInDocumentStore(store, 5);
				var provider = new RavenDBMembershipProvider();
				provider.DocumentStore = store;

				// Act
				Thread.Sleep(500);
				int totalRecords;
				var membershipUsers = provider.FindUsersByName("ser", 0, 10, out totalRecords); // Usernames are User1 .. Usern

				// Assert
				Assert.Equal(5, totalRecords); // All users should be returned
				Assert.Equal(5, membershipUsers.Count);
			}
		}

		[Fact]
		public void FindUsersWithPaging()
		{
			using (var store = NewInMemoryStore())
			{
				// Arrange
				CreateUsersInDocumentStore(store, 10);
				var provider = new RavenDBMembershipProvider();
				provider.DocumentStore = store;

				// Act
				Thread.Sleep(500);
				int totalRecords;
				var membershipUsers = provider.GetAllUsers(0, 5, out totalRecords);

				// Assert
				Assert.Equal(10, totalRecords); // All users should be returned
				Assert.Equal(5, membershipUsers.Count);
			}
		}

		[Fact]
		public void FindUsersForDomain()
		{
			using (var store = NewInMemoryStore())
			{
				// Arrange
				CreateUsersInDocumentStore(store, 10);
				var provider = new RavenDBMembershipProvider();
				provider.DocumentStore = store;

				// Act
				Thread.Sleep(500);
				int totalRecords;
				var membershipUsers = provider.FindUsersByEmail("@foo.bar", 0, 2, out totalRecords);
				int totalRecordsForUnknownDomain;
				var membershipUsersForUnknownDomain = provider.FindUsersByEmail("@foo.baz", 0, 2, out totalRecordsForUnknownDomain);

				// Assert
				Assert.Equal(10, totalRecords); // All users should be returned
				Assert.Equal(2, membershipUsers.Count);
				Assert.Equal(0, totalRecordsForUnknownDomain);
				Assert.Equal(0, membershipUsersForUnknownDomain.Count);
			}
		}

		private void CreateUsersInDocumentStore(IDocumentStore store, int numberOfUsers)
		{
			var users = CreateDummyUsers(numberOfUsers);
			using (var session = store.OpenSession())
			{
				foreach (var user in users)
				{
					session.Store(user);
				}
				session.SaveChanges();
			}
		}

        private static int PerProcessUnique = 0;

        public static User CreateDummyUser()
        {
            int i = ++PerProcessUnique;
            return new User {Username = String.Format("User{0}", i), Email = String.Format("User{0}@foo.bar", i)};
        }

		public static IList<User> CreateDummyUsers(int numberOfUsers)
		{
			var users = new List<User>(numberOfUsers);
			for (int i = 0; i < numberOfUsers; i++)
			{
				users.Add(CreateDummyUser());
			}
			return users;
		}
	}
}
