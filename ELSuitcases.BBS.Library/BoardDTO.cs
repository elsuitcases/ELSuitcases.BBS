using System;
using System.Collections.Generic;
using System.Text;

namespace ELSuitcases.BBS.Library
{
    public sealed class BoardDTO : DTO
    {
        public BoardDTO() : base()
        {
            PrimaryKeyPropertyNames = new string[1]
            {
                Constants.PROPERTY_KEY_NAME_BBS_ID
            };

            GenerateProperties();
        }



        private void GenerateProperties()
        {
            Add(Constants.PROPERTY_KEY_NAME_BBS_ID, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_BBS_NAME, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_BBS_TYPE, -1);
            Add(Constants.PROPERTY_KEY_NAME_CREATED_TIME, DateTime.MinValue);
            Add(Constants.PROPERTY_KEY_NAME_IS_ENABLED, "1");
        }
    }
}
