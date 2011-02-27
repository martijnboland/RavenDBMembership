using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;

namespace RavenDBMembership.Provider
{
    public abstract class MembershipValidationDecorator : MembershipProvider
    {
        private readonly MembershipProvider _original;
        private readonly IPasswordChecker _passwordChecker;

        public MembershipValidationDecorator(MembershipProvider original, IPasswordChecker passwordChecker)
        {
            _original = original;
            _passwordChecker = passwordChecker;
        }

        public new void Initialize(string name, NameValueCollection config)
        {
            _original.Initialize(name, config);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (!SecUtility.ValidateParameter(ref password, true, true, false, 0x80))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref username, true, true, true, 0x100))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref email, this.RequiresUniqueEmail, this.RequiresUniqueEmail, false, 0x100))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }
            if (password.Length < this.MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            int num = 0;
            for (int i = 0; i < password.Length; i++)
            {
                if (!char.IsLetterOrDigit(password, i))
                {
                    num++;
                }
            }
            if (num < this.MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(password, this.PasswordStrengthRegularExpression))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            this.OnValidatingPassword(e);
            if (e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            return _original.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public override string GetPassword(string username, string answer)
        {
            if (!this.EnablePasswordRetrieval)
            {
                throw new NotSupportedException("Membership_PasswordRetrieval_not_supported");
            }

            return _original.GetPassword(username, answer);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            /*
            int num;
            bool flag;
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            SecUtility.CheckParameter(ref oldPassword, true, true, false, 0x80, "oldPassword");
            SecUtility.CheckParameter(ref newPassword, true, true, false, 0x80, "newPassword");
            string salt = null;
            if (!_passwordChecker.CheckPassword(username, oldPassword, false))
            {
                return false;
            }
            if (newPassword.Length < this.MinRequiredPasswordLength)
            {
                throw new ArgumentException("Password is shorter than the minimum " + this.MinRequiredPasswordLength);
            }
            int num3 = 0;
            for (int i = 0; i < newPassword.Length; i++)
            {
                if (!char.IsLetterOrDigit(newPassword, i))
                {
                    num3++;
                }
            }
            if (num3 < this.MinRequiredNonAlphanumericCharacters)
            {
                throw new ArgumentException(SR.GetString("Password_need_more_non_alpha_numeric_chars", new object[] { "newPassword", this.MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture) }));
            }
            if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(newPassword, this.PasswordStrengthRegularExpression))
            {
                throw new ArgumentException(SR.GetString("Password_does_not_match_regular_expression", new object[] { "newPassword" }));
            }
            string objValue = base.EncodePassword(newPassword, num, salt);
            if (objValue.Length > 0x80)
            {
                throw new ArgumentException(SR.GetString("Membership_password_too_long"), "newPassword");
            }
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
            this.OnValidatingPassword(e);
            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                throw new ArgumentException(SR.GetString("Membership_Custom_Password_Validation_Failure"), "newPassword");
            }
            */
            return _original.ChangePassword(username, oldPassword, newPassword);
        }

        /*
        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
        {
            string str;
            int num;
            return this.CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved, out str, out num);
        }
         * */


        public override string ResetPassword(string username, string answer)
        {
            return _original.ResetPassword(username, answer);
        }

        public override void UpdateUser(MembershipUser user)
        {
            _original.UpdateUser(user);
        }

        public override bool ValidateUser(string username, string password)
        {
            return _original.ValidateUser(username, password);
        }

        public override bool UnlockUser(string userName)
        {
            return _original.UnlockUser(userName);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return _original.GetUser(providerUserKey, userIsOnline);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return _original.GetUser(username, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            return _original.GetUserNameByEmail(email);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return _original.DeleteUser(username, deleteAllRelatedData);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return _original.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            return _original.GetNumberOfUsersOnline();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return _original.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return _original.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _original.EnablePasswordRetrieval; }
        }

        public override bool EnablePasswordReset
        {
            get { return _original.EnablePasswordReset; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _original.RequiresQuestionAndAnswer; }
        }

        public override string ApplicationName
        {
            get { return _original.ApplicationName; }
            set { _original.ApplicationName = value; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _original.MaxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _original.PasswordAttemptWindow; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _original.RequiresUniqueEmail; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _original.PasswordFormat; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _original.MinRequiredPasswordLength; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _original.MinRequiredNonAlphanumericCharacters; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _original.PasswordStrengthRegularExpression; }
        }
    }
}
