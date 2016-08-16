using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using System.Security.Principal;
using System.Runtime.Serialization;
using GuildfordBoroughCouncil.Linq;
using System.Runtime.Caching;
using GuildfordBoroughCouncil.ActiveDirectory.Interfaces;
using System.DirectoryServices.AccountManagement;

namespace GuildfordBoroughCouncil.ActiveDirectory.Api.Lookup
{
    public static class Utils
    {
        public static int CountDirectReports(DirectoryEntry User)
        {
            int count = 0;

            if (User.Properties["directReports"].Value != null)
            {
                if (User.Properties["directReports"].Value.GetType() == typeof(string))
                {
                    count = 1;
                }
                else if (User.Properties["directReports"].Value.GetType() == typeof(object[]))
                {
                    count = ((object[])User.Properties["directReports"].Value).Count();
                }
            }

            return count;
        }
    }

    public class Group : IGroup
    {
        public Group() { }

        public Group(DirectoryEntry UserDirectoryEntry)
        {
            Name = (UserDirectoryEntry.Properties["displayName"].Value != null) ? UserDirectoryEntry.Properties["displayName"].Value.ToString() : UserDirectoryEntry.Properties["sAMAccountName"].Value.ToString();
            UserName = (UserDirectoryEntry.Properties["sAMAccountName"].Value != null) ? @"GUILDFORD\" + UserDirectoryEntry.Properties["sAMAccountName"].Value.ToString() : string.Empty;
            UserNameX = (UserDirectoryEntry.Properties["sAMAccountName"].Value != null) ? UserDirectoryEntry.Properties["sAMAccountName"].Value.ToString().ToLower() : string.Empty;
            Email = (UserDirectoryEntry.Properties["mail"].Value != null) ? UserDirectoryEntry.Properties["mail"].Value.ToString() : string.Empty;
            Description = (UserDirectoryEntry.Properties["description"].Value != null) ? UserDirectoryEntry.Properties["description"].Value.ToString() : string.Empty;
            PhotoUrl = (UserDirectoryEntry.Properties["extensionAttribute1"].Value != null) ? UserDirectoryEntry.Properties["extensionAttribute1"].Value.ToString() : Properties.Settings.Default.NoPhotoImageUrl;
        }

        public string UserName { get; set; }
        public string UserNameX { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public string Description { get; set; }
    }

    public class User : IUser
    {
        public User() { }

        public User(DirectoryEntry UserDirectoryEntry)
        {
            Name = (UserDirectoryEntry.Properties["displayName"].Value != null) ? UserDirectoryEntry.Properties["displayName"].Value.ToString() : UserDirectoryEntry.Properties["sAMAccountName"].Value.ToString();
            FirstName = (UserDirectoryEntry.Properties["givenName"].Value != null) ? UserDirectoryEntry.Properties["givenName"].Value.ToString() : string.Empty;
            LastName = (UserDirectoryEntry.Properties["sn"].Value != null) ? UserDirectoryEntry.Properties["sn"].Value.ToString() : string.Empty;
            UserName = (UserDirectoryEntry.Properties["sAMAccountName"].Value != null) ? @"GUILDFORD\" + UserDirectoryEntry.Properties["sAMAccountName"].Value.ToString() : string.Empty;
            UserNameX = (UserDirectoryEntry.Properties["sAMAccountName"].Value != null) ? UserDirectoryEntry.Properties["sAMAccountName"].Value.ToString().ToLower() : string.Empty;
            Department = (UserDirectoryEntry.Properties["department"].Value != null) ? UserDirectoryEntry.Properties["department"].Value.ToString() : string.Empty;
            Office = (UserDirectoryEntry.Properties["physicalDeliveryOfficeName"].Value != null) ? UserDirectoryEntry.Properties["physicalDeliveryOfficeName"].Value.ToString() : string.Empty;
            Title = (UserDirectoryEntry.Properties["title"].Value != null) ? UserDirectoryEntry.Properties["title"].Value.ToString() : string.Empty;
            Email = (UserDirectoryEntry.Properties["mail"].Value != null) ? UserDirectoryEntry.Properties["mail"].Value.ToString() : string.Empty;
            ManagerUserName = (UserDirectoryEntry.Properties["manager"].Value != null) ? UserDirectoryEntry.Properties["manager"].Value.ToString() : string.Empty;
            DirectReports = Utils.CountDirectReports(UserDirectoryEntry);
            Telephone = (UserDirectoryEntry.Properties["telephoneNumber"].Value != null) ? UserDirectoryEntry.Properties["telephoneNumber"].Value.ToString() : string.Empty;
            TelephoneXtn = (UserDirectoryEntry.Properties["telephoneNumber"].Value != null) ? UserDirectoryEntry.Properties["telephoneNumber"].Value.ToString().Replace(" ", string.Empty).Replace("01483444", "4").Replace("01483445", "5") : string.Empty;
            PhotoUrl = (UserDirectoryEntry.Properties["extensionAttribute1"].Value != null) ? UserDirectoryEntry.Properties["extensionAttribute1"].Value.ToString() : Properties.Settings.Default.NoPhotoImageUrl;

            if (!string.IsNullOrWhiteSpace(ManagerUserName))
            {
                DirectoryEntry ManagersEntry = new DirectoryEntry("LDAP://" + ManagerUserName);
                ManagerUserName = (ManagersEntry.Properties["sAMAccountName"].Value != null) ? @"GUILDFORD\" + ManagersEntry.Properties["sAMAccountName"].Value.ToString() : string.Empty;
                ManagerUserNameX = (ManagersEntry.Properties["sAMAccountName"].Value != null) ? ManagersEntry.Properties["sAMAccountName"].Value.ToString().ToLower() : string.Empty;
                ManagersEntry.Close();
            }
        }

        public string UserName { get; set; }
        public string UserNameX { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Office { get; set; }
        public string ManagerUserName { get; set; }
        public string ManagerUserNameX { get; set; }
        public int DirectReports { get; set; }

        public string Telephone { get; set; }
        public string TelephoneXtn { get; set; }
    }

    public static class Data
    {
        internal static string CleanQuery(string query)
        {
            return Regex.Replace(query ?? string.Empty, @"[^\w\s-]", string.Empty);
        }

        internal static IEnumerable<User> FindMembersByOU(string query = null, string Path = null)
        {
            if (Path == null)
            {
                return null;
            }

            var Members = new List<User>();

            DirectoryEntry UsersOu = new DirectoryEntry(Path);

            foreach (string s in UsersOu.Properties["member"])
            {
                // Only select users.
                if (s.Contains("GBC Users"))
                {
                    DirectoryEntry UserEntry = new DirectoryEntry("LDAP://" + s);

                    var UserData = new User(UserEntry);

                    UserEntry.Close();

                    Members.Add(UserData);
                }
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return Members.OrderBy(u => u.LastName);
            }
            else
            {
                return Members.Where(u => u.UserNameX.Contains(query, StringComparison.CurrentCultureIgnoreCase) || u.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) || u.Department.Contains(query, StringComparison.CurrentCultureIgnoreCase) || u.Office.Contains(query, StringComparison.CurrentCultureIgnoreCase) || u.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) || u.Telephone.Contains(query, StringComparison.CurrentCultureIgnoreCase)).OrderBy(u => u.LastName);
            }
        }

        //http://msdn.microsoft.com/en-us/library/windows/desktop/aa746475(v=vs.85).aspx
        //"(&(objectCategory=person)(objectClass=contact)(|(sn=Smith)(sn=Johnson)))"
        public static IEnumerable<User> HeadsOfService()
        {
            var c = MemoryCache.Default;

            var CachedUsers = (List<User>)c.Get("HeadsOfService");

            if (CachedUsers != null && CachedUsers.Count() > 0)
            {
                return CachedUsers.OrderBy(u => u.LastName);
            }

            var Users = FindMembersByOU(null, Properties.Settings.Default.ServiceHeadLdap).ToList();

            c.Add("HeadsOfService", Users, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddHours(6)) });

            return Users.OrderBy(u => u.LastName);
        }

        public static IEnumerable<User> Councillors(string query = null)
        {
            return FindMembersByOU(query, Properties.Settings.Default.CouncillorLdap);
        }

        public static IEnumerable<User> FindUsersByLastname(string query)
        {
            query = CleanQuery(query);

            var Users = new List<User>();

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Users;
            }

            DirectoryEntry UsersOu = new DirectoryEntry(Properties.Settings.Default.UserLdap);
            DirectorySearcher Search = new DirectorySearcher(UsersOu);
            Search.Filter = "(&(objectCategory=person)(objectClass=user)(sn=" + query + "*))";

            SearchResultCollection Results = Search.FindAll();

            foreach (SearchResult r in Results)
            {
                DirectoryEntry UserEntry = new DirectoryEntry(r.Path);

                var UserData = new User(UserEntry);

                UserEntry.Close();

                Users.Add(UserData);
            }

            return Users.OrderBy(u => u.LastName);
        }

        public static IEnumerable<User> FindUsers(string query)
        {
            query = CleanQuery(query);

            var Users = new List<User>();

            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
            {
                return Users;
            }

            DirectoryEntry UsersOu = new DirectoryEntry(Properties.Settings.Default.UserLdap);
            DirectorySearcher Search = new DirectorySearcher(UsersOu);
            // Remove "(objectClass=user)" to show contacts too
            Search.Filter = "(&(objectCategory=person)(objectClass=user)(|(sAMAccountName=" + query + ")(displayName=*" + query + "*)(department=" + query + "*)(physicalDeliveryOfficeName=" + query + "*)(title=" + query + "*)(telephoneNumber=*" + query + "*)(description=*" + query + "*)))";

            SearchResultCollection Results = Search.FindAll();

            foreach (SearchResult r in Results)
            {
                DirectoryEntry UserEntry = new DirectoryEntry(r.Path);

                try
                {
                    var UserData = new User(UserEntry);
                    Users.Add(UserData);
                }
                catch { }

                UserEntry.Close();
            }

            return Users.OrderBy(u => u.LastName);
        }

        public static IEnumerable<User> GroupMembers(string groupname)
        {
            var Users = new List<User>();

            DirectoryEntry Domain = new DirectoryEntry(Properties.Settings.Default.DomainLdap);

            DirectorySearcher Search = new DirectorySearcher(Domain);
            Search.Filter = "(&(sAMAccountName=" + groupname + ")(objectCategory=group))";

            SearchResult r = Search.FindOne();

            if (r != null)
            {
                string GroupDn = r.GetDirectoryEntry().Properties["distinguishedName"].Value.ToString();

                Search.Dispose();
                Search = new DirectorySearcher(Domain);
                Search.Filter = "(&(objectCategory=person)(objectClass=user)(memberOf=" + GroupDn + "))";

                SearchResultCollection Results = Search.FindAll();

                foreach (SearchResult m in Results)
                {
                    DirectoryEntry UserEntry = new DirectoryEntry(m.Path);

                    try
                    {
                        var UserData = new User(UserEntry);
                        Users.Add(UserData);
                    }
                    catch { }

                    UserEntry.Close();
                }
            }

            Search.Dispose();

            return Users.OrderBy(u => u.LastName);
        }

        public static IEnumerable<string> GroupSamAccountNamesForUser(string UserName, bool Follow = true)
        {
            var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, Properties.Settings.Default.DomainName, Properties.Settings.Default.DomainLdap), IdentityType.SamAccountName, UserName);

            if (Follow)
            {
                return user.GetAuthorizationGroups().Select(g => g.SamAccountName);
            }
            else
            {
                return user.GetGroups().Select(g => g.SamAccountName);
            }
        }

        public static IEnumerable<User> DirectReports(string username)
        {
            var Users = new List<User>();
            string ManagerDn = null; // This is the LDAP escaped value for null

            DirectoryEntry UsersOu = new DirectoryEntry(Properties.Settings.Default.UserLdap);
            DirectorySearcher Search = new DirectorySearcher(UsersOu);

            if (!string.IsNullOrWhiteSpace(username))
            {
                Search.Filter = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + username + "))";

                SearchResult Manager = Search.FindOne();

                if (Manager != null)
                {
                    ManagerDn = Manager.GetDirectoryEntry().Properties["distinguishedName"].Value.ToString();

                    //object[] DirectReports = Manager.GetDirectoryEntry().Properties["directReports"].Value as object[];
                    //var test2 = DirectReports;
                }

                Search.Dispose();
                Search = new DirectorySearcher(UsersOu);
                Search.Filter = "(&(objectCategory=person)(objectClass=user)(manager=" + ManagerDn + "))";
            }
            else
            {
                Search.Filter = "(&(objectCategory=person)(objectClass=user)(!(manager=*)))";
            }

            SearchResultCollection Results = Search.FindAll();

            foreach (SearchResult r in Results)
            {
                DirectoryEntry UserEntry = new DirectoryEntry(r.Path);

                try
                {
                    var UserData = new User(UserEntry);
                    Users.Add(UserData);
                }
                catch { }

                UserEntry.Close();
            }


            return Users.OrderBy(u => u.LastName);
        }

        public static IEnumerable<User> Users(IEnumerable<string> UserNames)
        {
            var Users = new List<User>();

            DirectoryEntry Domain = new DirectoryEntry(Properties.Settings.Default.DomainLdap);

            if (UserNames != null)
            {
                foreach (var u in UserNames)
                {
                    if (!string.IsNullOrWhiteSpace(u))
                    {
                        DirectorySearcher Search = new DirectorySearcher(Domain);
                        Search.Filter = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + u + "))";

                        SearchResult r = Search.FindOne();

                        if (r != null)
                        {
                            DirectoryEntry UserEntry = new DirectoryEntry(r.Path);

                            var UserData = new User(UserEntry);

                            UserEntry.Close();

                            Users.Add(UserData);
                        }

                        Search.Dispose();
                    }
                }
            }

            return Users;
        }

        public static IEnumerable<String> ServiceUnitNames(bool? IncludeXmt = false)
        {
            var ServiceUnits = Properties.Settings.Default.ServiceNames;

            if (IncludeXmt.HasValue && IncludeXmt.Value)
            {
                ServiceUnits.Add(Properties.Settings.Default.ManagementTeamName);
            }

            return ServiceUnits.Cast<string>().OrderBy(s => s);
        }

        public static IEnumerable<Group> FindGroups(string query)
        {
            query = CleanQuery(query);

            var Groups = new List<Group>();

            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
            {
                return Groups;
            }

            DirectoryEntry GroupsOu = new DirectoryEntry(Properties.Settings.Default.GroupLdap);
            DirectorySearcher Search = new DirectorySearcher(GroupsOu);
            Search.Filter = "(&(objectCategory=group)(|(sAMAccountName=" + query + ")(displayName=*" + query + "*)))";

            SearchResultCollection Results = Search.FindAll();

            foreach (SearchResult r in Results)
            {
                DirectoryEntry GroupEntry = new DirectoryEntry(r.Path);

                try
                {
                    var GroupData = new Group(GroupEntry);
                    Groups.Add(GroupData);
                }
                catch { }

                GroupEntry.Close();
            }

            return Groups.OrderBy(u => u.Name);
        }
        
        public static IEnumerable<User> FindHeadsOfService(string query)
        {
            return Lookup.Data.HeadsOfService().WhereIf(!string.IsNullOrWhiteSpace(query), u => u.Name.Contains(query) || u.Department.Contains(query) || u.Office.Contains(query) || u.Telephone.Contains(query) || u.Title.Contains(query));
        }

        public static IEnumerable<User> FindCouncillors(string query)
        {
            return Lookup.Data.Councillors().WhereIf(!string.IsNullOrWhiteSpace(query), u => u.Name.Contains(query) || u.Department.Contains(query) || u.Office.Contains(query) || u.Telephone.Contains(query) || u.Title.Contains(query));
        }

        public static IEnumerable<User> AllUsers()
        {
            var c = MemoryCache.Default;

            var CachedUsers = (List<User>)c.Get("AllUsers");

            if (CachedUsers != null && CachedUsers.Count() > 0)
            {
                return CachedUsers.OrderBy(u => u.LastName);
            }

            var Users = new List<User>();

            DirectoryEntry UsersOu = new DirectoryEntry(Properties.Settings.Default.UserLdap);
            DirectorySearcher Search = new DirectorySearcher(UsersOu);
            Search.Filter = "(&(objectCategory=person)(objectClass=user))";

            SearchResultCollection Results = Search.FindAll();

            foreach (SearchResult r in Results)
            {
                DirectoryEntry UserEntry = new DirectoryEntry(r.Path);

                try
                {
                    var UserData = new User(UserEntry);
                    Users.Add(UserData);
                }
                catch { }

                UserEntry.Close();
            }

            c.Add("AllUsers", Users, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddHours(6)) });

            return Users.OrderBy(u => u.LastName);
        }

        public static IEnumerable<Group> AllGroups()
        {
            var c = MemoryCache.Default;

            var CachedGroups = (List<Group>)c.Get("AllGroups");

            if (CachedGroups != null && CachedGroups.Count() > 0)
            {
                return CachedGroups.OrderBy(u => u.Name);
            }

            var Groups = new List<Group>();

            DirectoryEntry GroupsOu = new DirectoryEntry(Properties.Settings.Default.GroupLdap);
            DirectorySearcher Search = new DirectorySearcher(GroupsOu);
            Search.Filter = "(objectCategory=group)";

            SearchResultCollection Results = Search.FindAll();

            foreach (SearchResult r in Results)
            {
                DirectoryEntry GroupEntry = new DirectoryEntry(r.Path);

                try
                {
                    var GroupData = new Group(GroupEntry);
                    Groups.Add(GroupData);
                }
                catch { }

                GroupEntry.Close();
            }

            c.Add("AllGroups", Groups, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddHours(6)) });

            return Groups.OrderBy(u => u.Name);
        }
    }
}