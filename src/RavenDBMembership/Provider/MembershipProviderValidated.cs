using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using RavenDBMembership.UserStrings;

namespace RavenDBMembership.Provider
{
    public abstract class MembershipProviderValidated : MembershipProvider
    {
        public abstract MembershipUser CheckedCreateUser(string username, string password, string email,
                                                         string passwordQuestion, string passwordAnswer, bool isApproved,
                                                         object providerUserKey, out MembershipCreateStatus status);
        public abstract bool CheckPassword(string username, string password, bool updateLastLogin);
        public abstract bool CheckedChangePassword(string username, string oldPassword, string newPassword);
        public abstract bool CheckedDeleteUser(string username, bool deleteAllRelatedData);

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

            return CheckedCreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            SecUtility.CheckParameter(ref oldPassword, true, true, false, 0x80, "oldPassword");
            SecUtility.CheckParameter(ref newPassword, true, true, false, 0x80, "newPassword");

            if (!CheckPassword(username, oldPassword, false))
            {
                return false;
            }
            if (newPassword.Length < this.MinRequiredPasswordLength)
            {
                throw new ArgumentException("Password is shorter than the minimum " + this.MinRequiredPasswordLength, "newPassword");
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
                throw new ArgumentException(
                    SR.Password_need_more_non_alpha_numeric_chars_1.WithParameters(MinRequiredNonAlphanumericCharacters),
                    "newPassword");
            }
            if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(newPassword, this.PasswordStrengthRegularExpression))
            {
                throw new ArgumentException(SR.Password_does_not_match_regular_expression.WithParameters(),
                    "newPassword");
            }
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
            this.OnValidatingPassword(e);
            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                throw new ArgumentException(SR.Membership_Custom_Password_Validation_Failure.WithParameters(), "newPassword");
            }

            return CheckedChangePassword(username, oldPassword, newPassword);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");

            return CheckedDeleteUser(username, deleteAllRelatedData);
        }
    }
}
