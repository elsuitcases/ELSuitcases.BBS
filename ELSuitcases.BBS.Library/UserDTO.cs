using System;
using System.Collections.Generic;
using System.Text;

namespace ELSuitcases.BBS.Library
{
    public class UserDTO : DTO
    {
        public UserDTO() : base()
        {
            PrimaryKeyPropertyNames = new string[1]
            {
                Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID
            };

            GenerateProperties();
        }

        public UserDTO(string accountId, string fullName) : this()
        {
            this[Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID] = accountId;
            this[Constants.PROPERTY_KEY_NAME_CURRENT_USER_FULLNAME] = fullName;
            this[Constants.PROPERTY_KEY_NAME_CURRENT_USER_LOGIN_TIME] = DateTime.Now;
        }



        private void GenerateProperties()
        {
            Add(Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_CURRENT_USER_FULLNAME, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_CURRENT_USER_LOGIN_TIME, DateTime.MinValue);
        }
    }
}
