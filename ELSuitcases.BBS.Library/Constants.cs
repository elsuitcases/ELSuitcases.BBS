using System;
using System.Collections.Generic;
using System.Text;

namespace ELSuitcases.BBS.Library
{
    public static class Constants
    {
        public const string APPLICATION_NAME = "Bulletin Board System";

        public const char SEPARATOR_COMMA = ',';

        public const string API_ARTICLE_SUBPATH_DOWNLOAD_ENTRY_FILE = "file_entry";
        public const string API_ARTICLE_SUBPATH_DOWNLOAD_PACKAGE_FILE = "file";
        public const string API_ARTICLE_SUBPATH_UPLOAD_QUEUE = "upload/queue";
        public const string API_ARTICLE_SUBPATH_UPLOAD_DEQUEUE = "upload/dequeue";
        public const string API_ARTICLE_SUBPATH_UPLOAD_FILE = "upload/file";

        public const string CONFIG_KEY_NAME_API_HOSTNAME = "API_HostName";
        public const string CONFIG_KEY_NAME_API_PORT = "API_Port";
        public const string CONFIG_KEY_NAME_API_PROTOCOL = "API_Protocol";
        public const string CONFIG_KEY_NAME_API_SUB_PATH = "API_Sub_Path";

        public static readonly TimeSpan DEFAULT_VALUE_API_CLIENT_TIMEOUT = TimeSpan.FromMinutes(1);
        public const int DEFAULT_VALUE_ARTICLE_CONTENTS_MAX_LENGTH = 2048;
        public const int DEFAULT_VALUE_ATTACHED_FILE_ITEM_MAX_LENGTH = 102400000;
        public const int DEFAULT_VALUE_BUFFER_SIZE = 4096;
        public const int DEFAULT_VALUE_PAGER_PAGE_NO = 1;
        public const int DEFAULT_VALUE_PAGER_PAGE_SIZE = 10;
        public const char DEFAULT_VALUE_SEPARATOR = SEPARATOR_COMMA;

        public const string MESSAGE_ERROR_ARGUMENTS_NULL = "기능 실행에 필요한 요소가 제공되지 않았습니다.";
        public const string MESSAGE_ERROR_FUNCTION_ON_RUNNING = "기능 실행 중 오류가 발생하였습니다.";

        public const string PROPERTY_KEY_NAME_ARTICLE_ID = "ARTICLE_ID";
        public const string PROPERTY_KEY_NAME_ARTICLE_PASSWORD = "ARTICLE_PASSWORD";
        public const string PROPERTY_KEY_NAME_ARTICLE_TYPE = "ARTICLE_TYPE";
        public const string PROPERTY_KEY_NAME_ARTICLES_TOTAL_COUNT = "ARTICLES_TOTAL_COUNT";
        public const string PROPERTY_KEY_NAME_ARTICLE_REPLIES_TOTAL_COUNT = "ARTICLE_REPLIES_TOTAL_COUNT";
        public const string PROPERTY_KEY_NAME_ATTACHED_FILE_NAME = "ATTACHED_FILE_NAME";
        public const string PROPERTY_KEY_NAME_ATTACHED_FILES_COUNT = "ATTACHED_FILES_COUNT";
        public const string PROPERTY_KEY_NAME_ATTACHED_FILE_SIZE = "ATTACHED_FILE_SIZE";
        public const string PROPERTY_KEY_NAME_BBS_ID = "BBS_ID";
        public const string PROPERTY_KEY_NAME_BBS_NAME = "BBS_NAME";
        public const string PROPERTY_KEY_NAME_BBS_TYPE = "BBS_TYPE";
        public const string PROPERTY_KEY_NAME_CONTENTS = "CONTENTS";
        public const string PROPERTY_KEY_NAME_CREATED_TIME = "CREATED_TIME";
        public const string PROPERTY_KEY_NAME_IS_DELETED = "IS_DELETED";
        public const string PROPERTY_KEY_NAME_IS_ENABLED = "IS_ENABLED";
        public const string PROPERTY_KEY_NAME_PERCENT = "PERCENT";
        public const string PROPERTY_KEY_NAME_PRIMARY_KEY_PROPERTY_NAMES = "PrimaryKeyPropertyNames";
        public const string PROPERTY_KEY_NAME_READ_COUNT = "READ_COUNT";
        public const string PROPERTY_KEY_NAME_REPLY_ID = "REPLY_ID";
        public const string PROPERTY_KEY_NAME_ROW_NUMBER = "RID";
        public const string PROPERTY_KEY_NAME_TITLE = "TITLE";
        public const string PROPERTY_KEY_NAME_WRITER = "WRITER";
        public const string PROPERTY_KEY_NAME_WRITTEN_TIME = "WRITTEN_TIME";

        public const string PROPERTY_KEY_NAME_TRANSFER_ID = "TRANSFER_ID";
        public const string PROPERTY_KEY_NAME_UPLOAD_TASK = "UPLOAD_TASK";

        public const string PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID = "ACCOUNT_ID";
        public const string PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_PW = "ACCOUNT_PW";
        public const string PROPERTY_KEY_NAME_CURRENT_USER_EMAIL = "EMAIL";
        public const string PROPERTY_KEY_NAME_CURRENT_USER_FULLNAME = "FULLNAME";
        public const string PROPERTY_KEY_NAME_CURRENT_USER_LOGIN_TIME = "LOGIN_TIME";

        public const string PROPERTY_KEY_NAME_SEARCH_KEYWORD_TITLE = "KEYWORD_TITLE";



        public enum BBSArticleType : int
        {
            Unspecified = -1,
            General = 0
        }
        
        public enum BBSBoardType : int
        {
            Unspecified = -1,
            General = 0
        }
    }
}
