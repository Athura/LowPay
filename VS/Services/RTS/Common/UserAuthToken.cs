using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTS.Service.Common
{
	public class UserAuthToken : IEquatable<UserAuthToken>
	{
		public UserAuthToken()
		{
			UserName = "";
			Password = "";
			AuthTokenKey = "";
			ExpiresUTC = DateTime.MaxValue;
		}
		/// <summary>
		/// Active Directory User Name.
		/// </summary>
		public string UserName { get; set; }
		/// <summary>
		/// Active Directory Password.
		/// </summary>
		public string Password { get; set; }
		/// <summary>
		/// Expiration date (UTC) indicating when this token should expire.
		/// </summary>
		public DateTime ExpiresUTC { get; set; }
		/// <summary>
		/// String representation of the security token.
		/// </summary>
		public string AuthTokenKey { get; private set; }

		/// <summary>
		/// Assigns a key to the AuthTokenKey property.
		/// </summary>
		/// <remarks>
		/// If the key has already been generated then nothing is done.
		/// </remarks>
		/// <returns>The AuthTokenKey string value is returned.</returns>
		public string GenerateAuthTokenKey()
		{
			SecurityToken ST = new SecurityToken();
			ST.Key = Constants.SECRET_PREVIEW_HMAC_KEY;
			ST.Expires = ExpiresUTC;
			ST.Data["UserName"] = UserName;
			ST.Data["Password"] = Password;
			AuthTokenKey = ST.ToString();
			return AuthTokenKey;
		}

		#region FromAuthTokenKey
		/// <summary>
		/// Creates a UserAuthToken object from the key supplied.
		/// </summary>
		/// <param name="authTokenKey"></param>
		/// <returns>Returns the new object.  If the object cannot be created,
		/// then NULL is returned.
		/// </returns>
		public static UserAuthToken FromAuthTokenKey(string authTokenKey)
		{
			if ( !string.IsNullOrWhiteSpace(authTokenKey) )
			{
				SecurityToken ST = SecurityToken.FromString(Constants.SECRET_PREVIEW_HMAC_KEY, authTokenKey);
				UserAuthToken TR = new UserAuthToken()
				{
					AuthTokenKey = authTokenKey,
					UserName = ST.Data["UserName"],
					Password = ST.Data["Password"],
					ExpiresUTC = ST.Expires
				};
				return TR;
			}
			return null;
		}
		#endregion

		#region ExecuteAsUser
		public void ExecuteAsUser(Action toExecute)
		{
			toExecute.Invoke();
			//DataManagementContext.SubContext(() =>
			//{
			//    DataManagementContext.Current.LoggedInUser = User;
			//    toExecute.Invoke();
			//});
		}
		public T ExecuteAsUser<T>(Func<T> func)
		{
			return func.WrapAction(A => ExecuteAsUser(A)).Invoke();
		}
		#endregion

		#region Comparison overrides
		public bool Equals(UserAuthToken other)
		{
			// If 'other' is null then return false.
			if ( Object.ReferenceEquals(other, null) )
				return false;
			// If both object 'reference' the same object, then return true.
			if ( Object.ReferenceEquals(this, other) )
				return true;
			// If run-time types are not exactly the same, return false.
			if ( this.GetType() != other.GetType() )
				return false;
			return AuthTokenKey.Equals(other.AuthTokenKey);
		}
		public override bool Equals(object other)
		{
			return this.Equals(other as UserAuthToken);
		}
		public static bool operator ==(UserAuthToken luat, UserAuthToken ruat)
		{
			// Check for null on left side.
			if ( Object.ReferenceEquals(luat, null) )
			{
				// null == null is true, otherwise return false
				return Object.ReferenceEquals(ruat, null);
			}
			// Equals handles case of null on right side.
			return luat.Equals(ruat);
		}
		public static bool operator !=(UserAuthToken luat, UserAuthToken ruat)
		{
			return !(luat == ruat);
		}
		#endregion

		#region GetHashCode
		public override int GetHashCode()
		{
			return AuthTokenKey.GetHashCode();
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return AuthTokenKey;
		}
		#endregion

	}
}
