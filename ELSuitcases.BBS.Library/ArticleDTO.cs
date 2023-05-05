using System;
using System.Collections.Generic;
using System.Text;

namespace ELSuitcases.BBS.Library
{
    public sealed class ArticleDTO : DTO
    {
        public ArticleDTO() : base()
        {
            PrimaryKeyPropertyNames = new string[3]
            {
                Constants.PROPERTY_KEY_NAME_BBS_ID,
                Constants.PROPERTY_KEY_NAME_ARTICLE_ID,
                Constants.PROPERTY_KEY_NAME_REPLY_ID
            };

            GenerateProperties();
        }

        public ArticleDTO(string bbsId, Constants.BBSArticleType articleType, string title, string contents, string articlePassword, string writer) : this()
        {
            SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, bbsId);
            SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_TYPE, articleType);
            SetValue(Constants.PROPERTY_KEY_NAME_TITLE, title);
            SetValue(Constants.PROPERTY_KEY_NAME_CONTENTS, contents);
            SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD, articlePassword);
            SetValue(Constants.PROPERTY_KEY_NAME_WRITER, writer);
        }



        private void GenerateProperties()
        {
            Add(Constants.PROPERTY_KEY_NAME_ROW_NUMBER, -1);
            Add(Constants.PROPERTY_KEY_NAME_BBS_ID, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_ARTICLE_ID, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_REPLY_ID, -1);
            Add(Constants.PROPERTY_KEY_NAME_ARTICLE_TYPE, -1);
            Add(Constants.PROPERTY_KEY_NAME_TITLE, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_WRITER, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_WRITTEN_TIME, DateTime.MinValue);
            Add(Constants.PROPERTY_KEY_NAME_READ_COUNT, -1);
            Add(Constants.PROPERTY_KEY_NAME_IS_DELETED, "0");
            Add(Constants.PROPERTY_KEY_NAME_CONTENTS, string.Empty);
            Add(Constants.PROPERTY_KEY_NAME_ATTACHED_FILES_COUNT, 0);
            Add(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME, string.Empty);
        }
    }
}
