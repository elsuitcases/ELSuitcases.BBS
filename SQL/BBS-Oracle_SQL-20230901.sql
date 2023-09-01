--------------------------------------------------------
--  DDL for Table BBSARTICLE
--------------------------------------------------------

  CREATE TABLE "BBS"."BBSARTICLE" 
   (	"BBS_ID" VARCHAR2(32 BYTE), 
	"ARTICLE_ID" VARCHAR2(16 BYTE), 
	"REPLY_ID" NUMBER(*,0) DEFAULT 0, 
	"ARTICLE_TYPE" NUMBER(*,0), 
	"TITLE" VARCHAR2(512 BYTE), 
	"WRITER" VARCHAR2(32 BYTE), 
	"WRITTEN_TIME" DATE, 
	"READ_COUNT" NUMBER(*,0) DEFAULT 0, 
	"IS_DELETED" CHAR(1 BYTE) DEFAULT '0', 
	"ARTICLE_PASSWORD" VARCHAR2(128 BYTE)
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Table BBSBOARD
--------------------------------------------------------

  CREATE TABLE "BBS"."BBSBOARD" 
   (	"BBS_ID" VARCHAR2(32 BYTE), 
	"BBS_NAME" VARCHAR2(128 BYTE), 
	"BBS_TYPE" NUMBER(*,0), 
	"CREATED_TIME" DATE, 
	"IS_ENABLED" CHAR(1 BYTE) DEFAULT '1'
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Table BBSCONTENT
--------------------------------------------------------

  CREATE TABLE "BBS"."BBSCONTENT" 
   (	"BBS_ID" VARCHAR2(32 BYTE), 
	"ARTICLE_ID" VARCHAR2(16 BYTE), 
	"REPLY_ID" NUMBER(*,0), 
	"CONTENTS" NCLOB
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" 
 LOB ("CONTENTS") STORE AS BASICFILE (
  TABLESPACE "USERS" ENABLE STORAGE IN ROW CHUNK 8192 RETENTION 
  NOCACHE LOGGING 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)) ;
--------------------------------------------------------
--  DDL for Table BBSMEMBER
--------------------------------------------------------

  CREATE TABLE "BBS"."BBSMEMBER" 
   (	"MEMBER_ID" VARCHAR2(32 BYTE), 
	"ACCOUNT_ID" VARCHAR2(32 BYTE), 
	"ACCOUNT_PW" VARCHAR2(1024 BYTE), 
	"EMAIL" VARCHAR2(1024 BYTE), 
	"CREATED_TIME" DATE, 
	"IS_ENABLED" CHAR(1 BYTE) DEFAULT '1', 
	"FULLNAME" NVARCHAR2(64)
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
 

   COMMENT ON COLUMN "BBS"."BBSMEMBER"."MEMBER_ID" IS '회원 고유번호';
 
   COMMENT ON COLUMN "BBS"."BBSMEMBER"."ACCOUNT_ID" IS '계정 ID';
 
   COMMENT ON COLUMN "BBS"."BBSMEMBER"."ACCOUNT_PW" IS '계정 암호';
 
   COMMENT ON COLUMN "BBS"."BBSMEMBER"."EMAIL" IS 'E-mail 주소';
 
   COMMENT ON COLUMN "BBS"."BBSMEMBER"."CREATED_TIME" IS '생성일시';
 
   COMMENT ON COLUMN "BBS"."BBSMEMBER"."IS_ENABLED" IS '사용가능여부';
 
   COMMENT ON COLUMN "BBS"."BBSMEMBER"."FULLNAME" IS '이름';
--------------------------------------------------------
--  DDL for Index BBSARTICLE_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "BBS"."BBSARTICLE_PK" ON "BBS"."BBSARTICLE" ("BBS_ID", "ARTICLE_ID", "REPLY_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Index IDX_BBSARTICLE
--------------------------------------------------------

  CREATE INDEX "BBS"."IDX_BBSARTICLE" ON "BBS"."BBSARTICLE" ("BBS_ID", "ARTICLE_ID" DESC, "REPLY_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Index BBSBOARD_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "BBS"."BBSBOARD_PK" ON "BBS"."BBSBOARD" ("BBS_ID", "BBS_TYPE") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Index IDX_BBSBOARD
--------------------------------------------------------

  CREATE INDEX "BBS"."IDX_BBSBOARD" ON "BBS"."BBSBOARD" ("BBS_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Index BBSCONTENT_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "BBS"."BBSCONTENT_PK" ON "BBS"."BBSCONTENT" ("BBS_ID", "ARTICLE_ID", "REPLY_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Index IDX_BBSCONTENT
--------------------------------------------------------

  CREATE INDEX "BBS"."IDX_BBSCONTENT" ON "BBS"."BBSCONTENT" ("BBS_ID", "ARTICLE_ID" DESC, "REPLY_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  DDL for Index BBSMEMBER_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "BBS"."BBSMEMBER_PK" ON "BBS"."BBSMEMBER" ("MEMBER_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS" ;
--------------------------------------------------------
--  Constraints for Table BBSARTICLE
--------------------------------------------------------

  ALTER TABLE "BBS"."BBSARTICLE" ADD CONSTRAINT "BBSARTICLE_PK" PRIMARY KEY ("BBS_ID", "ARTICLE_ID", "REPLY_ID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS"  ENABLE;
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("BBS_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("ARTICLE_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("REPLY_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("ARTICLE_TYPE" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("TITLE" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("WRITER" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("WRITTEN_TIME" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSARTICLE" MODIFY ("IS_DELETED" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table BBSBOARD
--------------------------------------------------------

  ALTER TABLE "BBS"."BBSBOARD" ADD CONSTRAINT "BBSBOARD_PK" PRIMARY KEY ("BBS_ID", "BBS_TYPE")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS"  ENABLE;
 
  ALTER TABLE "BBS"."BBSBOARD" MODIFY ("BBS_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSBOARD" MODIFY ("BBS_NAME" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSBOARD" MODIFY ("BBS_TYPE" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSBOARD" MODIFY ("CREATED_TIME" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSBOARD" MODIFY ("IS_ENABLED" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table BBSCONTENT
--------------------------------------------------------

  ALTER TABLE "BBS"."BBSCONTENT" ADD CONSTRAINT "BBSCONTENT_PK" PRIMARY KEY ("BBS_ID", "ARTICLE_ID", "REPLY_ID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS"  ENABLE;
 
  ALTER TABLE "BBS"."BBSCONTENT" MODIFY ("BBS_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSCONTENT" MODIFY ("ARTICLE_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSCONTENT" MODIFY ("REPLY_ID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table BBSMEMBER
--------------------------------------------------------

  ALTER TABLE "BBS"."BBSMEMBER" ADD CONSTRAINT "BBSMEMBER_PK" PRIMARY KEY ("MEMBER_ID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "USERS"  ENABLE;
 
  ALTER TABLE "BBS"."BBSMEMBER" MODIFY ("MEMBER_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSMEMBER" MODIFY ("ACCOUNT_ID" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSMEMBER" MODIFY ("CREATED_TIME" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSMEMBER" MODIFY ("IS_ENABLED" NOT NULL ENABLE);
 
  ALTER TABLE "BBS"."BBSMEMBER" MODIFY ("FULLNAME" NOT NULL ENABLE);



--------------------------------------------------------
--  DDL for Package PKG_BBS_ARTICLE
--------------------------------------------------------

  CREATE OR REPLACE PACKAGE "BBS"."PKG_BBS_ARTICLE" 
AS    
    TYPE cv_result IS REF CURSOR;

    PROCEDURE List_By_Page
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_keyword_t         IN      VARCHAR2,
        p_page_no           IN      INTEGER,
        p_page_size         IN      INTEGER,
        p_result            OUT     cv_result,
        p_total_count       OUT     INTEGER,
        p_page_count        OUT     INTEGER
    );

    PROCEDURE List_All
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_result            OUT     cv_result
    );

    PROCEDURE Delete_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_reply_id          IN      bbsarticle.reply_id%TYPE,
        p_result            OUT     INTEGER
    );

    PROCEDURE Read_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_reply_id          IN      bbsarticle.reply_id%TYPE,
        p_is_increase_count IN      CHAR,
        p_result            OUT     cv_result
    );

    PROCEDURE Reply_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_article_type      IN      bbsarticle.article_type%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_writer            IN      bbsarticle.writer%TYPE,
        p_written_time      IN      bbsarticle.written_time%TYPE,
        p_article_password  IN      bbsarticle.article_password%TYPE,
        p_contents          IN      bbscontent.contents%TYPE,
        p_result            OUT     INTEGER,
        p_reply_id_new      OUT     bbsarticle.reply_id%TYPE
    );

    PROCEDURE Search_Article_By_Title
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_result            OUT     cv_result
    );

    PROCEDURE Update_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_reply_id          IN      bbsarticle.reply_id%TYPE,
        p_article_type      IN      bbsarticle.article_type%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_article_password  IN      bbsarticle.article_password%TYPE,
        p_is_deleted        IN      bbsarticle.is_deleted%TYPE,
        p_contents          IN      bbscontent.contents%TYPE,
        p_result            OUT     INTEGER
    );

    PROCEDURE Write_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_article_type      IN      bbsarticle.article_type%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_writer            IN      bbsarticle.writer%TYPE,
        p_written_time      IN      bbsarticle.written_time%TYPE,
        p_article_password  IN      bbsarticle.article_password%TYPE,
        p_contents          IN      bbscontent.contents%TYPE,
        p_result            OUT     INTEGER,
        p_reply_id_new      OUT     bbsarticle.reply_id%TYPE
    );

END PKG_BBS_Article;


/
--------------------------------------------------------
--  DDL for Package PKG_BBS_BOARD
--------------------------------------------------------

  CREATE OR REPLACE PACKAGE "BBS"."PKG_BBS_BOARD" 
AS 
    TYPE cv_result IS REF CURSOR;

    PROCEDURE List_All
    (
        p_result            OUT     cv_result
    );

    PROCEDURE Create_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_bbs_name          IN      bbsboard.bbs_name%TYPE,
        p_bbs_type          IN      bbsboard.bbs_type%TYPE,
        p_created_time      IN      bbsboard.created_time%TYPE,
        p_result            OUT     INTEGER
    );

    PROCEDURE Delete_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_result            OUT     INTEGER
    );

    PROCEDURE Get_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_result            OUT     cv_result
    );

    PROCEDURE Update_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_bbs_name          IN      bbsboard.bbs_name%TYPE,
        p_bbs_type          IN      bbsboard.bbs_type%TYPE,
        p_is_enabled        IN      bbsboard.is_enabled%TYPE,
        p_result            OUT     INTEGER
    );

END PKG_BBS_Board;


/
--------------------------------------------------------
--  DDL for Package PKG_BBS_MEMBER
--------------------------------------------------------

  CREATE OR REPLACE PACKAGE "BBS"."PKG_BBS_MEMBER" 
AS 
    TYPE cv_result IS REF CURSOR;
    
    PROCEDURE Get_By_Account_IDPW
    (
        p_account_id        IN      bbsmember.account_id%TYPE,
        p_account_pw        IN      bbsmember.account_pw%TYPE,
        p_result            OUT     cv_result
    );
    
END PKG_BBS_MEMBER;

/



--------------------------------------------------------
--  DDL for Package Body PKG_BBS_ARTICLE
--------------------------------------------------------

  CREATE OR REPLACE PACKAGE BODY "BBS"."PKG_BBS_ARTICLE" AS

    PROCEDURE List_By_Page
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_keyword_t         IN      VARCHAR2,
        p_page_no           IN      INTEGER,
        p_page_size         IN      INTEGER,
        p_result            OUT     cv_result,
        p_total_count       OUT     INTEGER,
        p_page_count        OUT     INTEGER
    )
    AS
        v_page_no           INTEGER         := 1;
        v_index_start       INTEGER         := 1;
        v_index_end         INTEGER         := 1;
        v_keyword_t         VARCHAR2(200)   := '';
    BEGIN
        v_page_no   := p_page_no;
        v_keyword_t := LOWER(p_keyword_t);

        SELECT 
                COUNT(article_id)
            INTO p_total_count
            FROM bbsarticle
            WHERE 
                bbs_id = p_bbs_id AND reply_id = 0 AND is_deleted <> '1' AND LOWER(title) LIKE '%' || v_keyword_t || '%';

        IF p_total_count < 1 THEN
            p_page_count := 0;
            RETURN;
        END IF;

        p_page_count := p_total_count / p_page_size;
        IF MOD(p_total_count, p_page_size) > 0 THEN
            p_page_count := p_page_count + 1;
        END IF;

        IF v_page_no > p_page_count OR v_page_no < 1 THEN
            v_page_no := 1;
        END IF;

        --v_index_end := v_page_no * p_page_size;
        --v_index_start := v_index_end - p_page_size + 1;

        v_index_start := p_total_count - (v_page_no * p_page_size) + 1;
        IF v_index_start < 1 THEN
            v_index_start := 1;
        END IF;

        v_index_end := v_index_start + p_page_size - 1;
        IF v_index_end > p_total_count THEN
            v_index_end := p_total_count;
        END IF;

        DBMS_OUTPUT.ENABLE;
        DBMS_OUTPUT.PUT_LINE('v_index_start : ' || v_index_start);
        DBMS_OUTPUT.PUT_LINE('v_index_end : ' || v_index_end);

        OPEN p_result FOR 
            WITH 
            tbl_aid AS (
                SELECT ROWNUM rid, a.*
                FROM 
                    (
                        SELECT 
                            DISTINCT(article_id) aid, written_time
                        FROM bbsarticle 
                        WHERE
                            bbs_id = p_bbs_id AND is_deleted <> '1' AND LOWER(title) LIKE '%' || v_keyword_t || '%'
                        ORDER BY written_time ASC
                    ) a
                --ORDER BY rid DESC
            )
            SELECT 
                    r.rid,
                    a.bbs_id,
                    a.article_id,
                    a.reply_id,
                    a.article_type,
                    a.title,
                    a.writer,
                    a.written_time,
                    a.read_count,
                    a.is_deleted,
                    a.article_password,
                    '' contents,
                    p_total_count AS "ARTICLES_TOTAL_COUNT",
                    0 AS "ATTACHED_FILES_COUNT"
                FROM 
                    tbl_aid r,
                    bbsarticle a
                WHERE 
                    (r.rid BETWEEN v_index_start AND v_index_end) AND
                    a.article_id = r.aid
                ORDER BY r.rid DESC, a.reply_id ASC;
    END List_By_Page;

    PROCEDURE List_All
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_result            OUT     cv_result
    ) 
    AS
    BEGIN
        IF p_bbs_id IS NOT NULL AND LENGTH(TRIM(p_bbs_id)) > 0 THEN
            OPEN p_result FOR
                WITH 
                tbl_aid AS (
                    SELECT ROWNUM rid, a.*
                    FROM 
                        (
                            SELECT 
                                article_id aid 
                            FROM bbsarticle 
                            WHERE
                                bbs_id = p_bbs_id AND reply_id = 0 AND is_deleted <> 1    
                        ) a
                )
                SELECT 
                    r.rid,
                    a.bbs_id,
                    a.article_id,
                    a.reply_id,
                    a.article_type,
                    a.title,
                    a.writer,
                    a.written_time,
                    a.read_count,
                    a.is_deleted,
                    a.article_password,
                    '' contents,
                    (
                        SELECT COUNT(rid) FROM tbl_aid
                    ) AS "ARTICLES_TOTAL_COUNT",
                    0 AS "ATTACHED_FILES_COUNT"
                FROM 
                    tbl_aid r,
                    bbsarticle a
                WHERE 
                    a.article_id = r.aid
                ORDER BY r.rid DESC;
        ELSE
            OPEN p_result FOR
                WITH 
                tbl_aid AS (
                    SELECT ROWNUM rid, a.*
                    FROM 
                        (
                            SELECT 
                                article_id aid 
                            FROM bbsarticle 
                            WHERE reply_id = 0 AND is_deleted <> 1
                            ORDER BY written_time ASC
                        ) a    
                )
                SELECT 
                    r.rid,
                    a.bbs_id,
                    a.article_id,
                    a.reply_id,
                    a.article_type,
                    a.title,
                    a.writer,
                    a.written_time,
                    a.read_count,
                    a.is_deleted,
                    a.article_password,
                    '' contents,
                    (
                        SELECT COUNT(rid) FROM tbl_aid
                    ) AS "ARTICLES_TOTAL_COUNT",
                    0 AS "ATTACHED_FILES_COUNT"
                FROM 
                    tbl_aid r,
                    bbsarticle a
                WHERE 
                    a.article_id = r.aid
                ORDER BY r.rid DESC;
        END IF;
    END List_All;

    PROCEDURE Delete_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_reply_id          IN      bbsarticle.reply_id%TYPE,
        p_result            OUT     INTEGER
    )
    AS
        v_count     INTEGER             := 0;
        v_errcode   INTEGER             := 0;
        v_errm      VARCHAR2(4096)      := '';
    BEGIN
        SELECT COUNT(article_id) 
            INTO v_count
            FROM bbsarticle
            WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

        IF v_count > 0 THEN
            UPDATE bbsarticle
                SET is_deleted = '1'
                WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                COMMIT;
                v_count := 1;
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF;
        ELSE
            v_count := 0;
        END IF;

        p_result := v_count;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                p_result := -1;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Delete_Article;

    PROCEDURE Read_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_reply_id          IN      bbsarticle.reply_id%TYPE,
        p_is_increase_count IN      CHAR,
        p_result            OUT     cv_result
    ) 
    AS
        v_count     INTEGER     := 0;
        v_is_plus   CHAR(200)   := '1';
    BEGIN
        SELECT COUNT(article_id) 
            INTO v_count 
            FROM bbsarticle 
            WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

        IF v_count < 1 THEN
            RETURN;
        END IF;

        v_count := 0;
        v_is_plus := UPPER(p_is_increase_count);

        IF v_is_plus IN ('1', 'Y', 'T', 'TRUE') THEN
            SELECT read_count 
                INTO v_count
                FROM bbsarticle
                WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

            UPDATE bbsarticle
                SET read_count = v_count + 1
                WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;
            COMMIT;
        END IF;

        OPEN p_result FOR
            SELECT 
                    a.bbs_id,
                    a.article_id,
                    a.reply_id,
                    a.article_type,
                    a.title,
                    a.writer,
                    a.written_time,
                    a.read_count,
                    a.is_deleted,
                    a.article_password,
                    c.contents  
                FROM bbsarticle a
                JOIN bbscontent c
                ON a.bbs_id = c.bbs_id AND a.article_id = c.article_id AND a.reply_id = c.reply_id
                WHERE a.bbs_id = p_bbs_id AND a.article_id = p_article_id AND a.reply_id = p_reply_id;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || SQLCODE || ' ' || SQLERRM(SQLCODE));
    END Read_Article;

    PROCEDURE Reply_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_article_type      IN      bbsarticle.article_type%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_writer            IN      bbsarticle.writer%TYPE,
        p_written_time      IN      bbsarticle.written_time%TYPE,
        p_article_password  IN      bbsarticle.article_password%TYPE,
        p_contents          IN      bbscontent.contents%TYPE,
        p_result            OUT     INTEGER,
        p_reply_id_new      OUT     bbsarticle.reply_id%TYPE
    ) 
    AS
        v_count         INTEGER                     := 0;
        v_new_reply_id  bbsarticle.reply_id%TYPE    := -1;
        v_errcode       INTEGER                     := 0;
        v_errm          VARCHAR2(4096)              := '';
    BEGIN
        SELECT COUNT(article_id)
            INTO v_count
            FROM bbsarticle
            WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = 0;

        IF v_count < 1 THEN
            v_count := 0;
        ELSE
            SELECT MAX(reply_id) + 1 INTO v_new_reply_id FROM bbsarticle WHERE bbs_id = p_bbs_id AND article_id = p_article_id;

            INSERT INTO bbsarticle
            (
                bbs_id,
                article_id,
                reply_id,
                article_type,
                title,
                writer,
                written_time,
                read_count,
                is_deleted,
                article_password
            )
            VALUES
            (
                p_bbs_id,
                p_article_id,
                v_new_reply_id,
                p_article_type,
                p_title,
                p_writer,
                p_written_time,
                0,
                '0',
                p_article_password
            );

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                INSERT INTO bbscontent
                (
                    bbs_id,
                    article_id,
                    reply_id,
                    contents
                )
                VALUES
                (
                    p_bbs_id,
                    p_article_id,
                    v_new_reply_id,
                    p_contents
                );

                v_count := SQL%ROWCOUNT;

                IF v_count > 0 THEN
                    COMMIT;
                    v_count := 1;
                ELSE
                    ROLLBACK;
                    v_count := 0;
                END IF;
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF;            
        END IF;

        p_result := v_count;
        p_reply_id_new := v_new_reply_id;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                p_result := -1;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Reply_Article;

    PROCEDURE Search_Article_By_Title
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_result            OUT     cv_result
    ) 
    AS
    BEGIN
        OPEN p_result FOR
            SELECT * 
                FROM bbsarticle
                WHERE bbs_id = p_bbs_id AND title LIKE '%' || p_title || '%'
                ORDER BY article_id DESC, reply_id ASC;
    END Search_Article_By_Title;

    PROCEDURE Update_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_reply_id          IN      bbsarticle.reply_id%TYPE,
        p_article_type      IN      bbsarticle.article_type%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_article_password  IN      bbsarticle.article_password%TYPE,
        p_is_deleted        IN      bbsarticle.is_deleted%TYPE,
        p_contents          IN      bbscontent.contents%TYPE,
        p_result            OUT     INTEGER
    )
    AS
        v_count     INTEGER              := 0;
        v_errcode   INTEGER              := 0;
        v_errm      VARCHAR2(4096)       := '';
    BEGIN
        SELECT COUNT(article_id)
            INTO v_count
            FROM bbsarticle
            WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

        IF v_count < 1 THEN
            v_count := 0;
        ELSE
            UPDATE bbsarticle
                SET
                    article_type = p_article_type,
                    title = p_title,
                    article_password = p_article_password,
                    is_deleted = p_is_deleted
                WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                UPDATE bbscontent
                    SET contents = p_contents
                    WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = p_reply_id;

                v_count := SQL%ROWCOUNT;

                IF v_count > 0 THEN
                    COMMIT;
                    v_count := 1;
                ELSE
                    ROLLBACK;
                    v_count := 0;
                END IF;    
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF;
        END IF;

        p_result := v_count;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                p_result := -1;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Update_Article;

    PROCEDURE Write_Article
    (
        p_bbs_id            IN      bbsarticle.bbs_id%TYPE,
        p_article_id        IN      bbsarticle.article_id%TYPE,
        p_article_type      IN      bbsarticle.article_type%TYPE,
        p_title             IN      bbsarticle.title%TYPE,
        p_writer            IN      bbsarticle.writer%TYPE,
        p_written_time      IN      bbsarticle.written_time%TYPE,
        p_article_password  IN      bbsarticle.article_password%TYPE,
        p_contents          IN      bbscontent.contents%TYPE,
        p_result            OUT     INTEGER,
        p_reply_id_new      OUT     bbsarticle.reply_id%TYPE
    ) 
    AS
        v_count         INTEGER                     := 0;
        v_new_reply_id  bbsarticle.reply_id%TYPE    := -1;
        v_errcode       INTEGER                     := 0;
        v_errm          VARCHAR2(4096)              := '';
    BEGIN
        SELECT COUNT(article_id)
            INTO v_count
            FROM bbsarticle
            WHERE bbs_id = p_bbs_id AND article_id = p_article_id AND reply_id = 0;

        IF v_count >= 1 THEN
            v_count := 0;
        ELSE
            v_new_reply_id := 0;

            INSERT INTO bbsarticle
            (
                bbs_id,
                article_id,
                reply_id,
                article_type,
                title,
                writer,
                written_time,
                read_count,
                is_deleted,
                article_password
            )
            VALUES
            (
                p_bbs_id,
                p_article_id,
                v_new_reply_id,
                p_article_type,
                p_title,
                p_writer,
                p_written_time,
                0,
                '0',
                p_article_password
            );

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                INSERT INTO bbscontent
                (
                    bbs_id,
                    article_id,
                    reply_id,
                    contents
                )
                VALUES
                (
                    p_bbs_id,
                    p_article_id,
                    v_new_reply_id,
                    p_contents
                );

                v_count := SQL%ROWCOUNT;

                IF v_count > 0 THEN
                    COMMIT;
                    v_count := 1;
                ELSE
                    ROLLBACK;
                    v_count := 0;
                END IF;
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF; 
        END IF;

        p_result := v_count;
        p_reply_id_new := v_new_reply_id;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                p_result := -1;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Write_Article;

END PKG_BBS_ARTICLE;


/

--------------------------------------------------------
--  DDL for Package Body PKG_BBS_BOARD
--------------------------------------------------------

  CREATE OR REPLACE PACKAGE BODY "BBS"."PKG_BBS_BOARD" AS

    PROCEDURE List_All
    (
        p_result            OUT     cv_result
    ) 
    AS
    BEGIN
        OPEN p_result FOR
            SELECT 
                    b.bbs_id,
                    b.bbs_name,
                    b.bbs_type,
                    b.created_time,
                    b.is_enabled,
                    (SELECT COUNT(article_id) FROM bbsarticle WHERE bbs_id = b.bbs_id AND is_deleted <> '1') AS "ARTICLES_TOTAL_COUNT" 
                FROM bbsboard b
                WHERE is_enabled = '1';
                --ORDER BY b.bbs_id ASC;
    END List_All;

    PROCEDURE Create_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_bbs_name          IN      bbsboard.bbs_name%TYPE,
        p_bbs_type          IN      bbsboard.bbs_type%TYPE,
        p_created_time      IN      bbsboard.created_time%TYPE,
        p_result            OUT     INTEGER
    ) 
    AS
        v_count     INTEGER             := 0;
        v_errcode   INTEGER             := 0;
        v_errm      VARCHAR2(4096)      := '';
    BEGIN
        SELECT COUNT(bbs_id) 
            INTO v_count
            FROM bbsboard
            WHERE bbs_id = p_bbs_id;

        IF v_count < 1 THEN
            INSERT INTO bbsboard
            (
                bbs_id,
                bbs_name,
                bbs_type,
                created_time,
                is_enabled
            )
            VALUES
            (
                p_bbs_id,
                p_bbs_name,
                p_bbs_type,
                p_created_time,
                '1'
            );

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                COMMIT;
                v_count := 1;
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF;
        ELSE
            v_count := 0;
        END IF;

        p_result := v_count;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                p_result := -1;
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Create_Board;

    PROCEDURE Delete_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_result            OUT     INTEGER
    ) 
    AS
        v_count     INTEGER             := 0;
        v_errcode   INTEGER             := 0;
        v_errm      VARCHAR2(4096)      := '';
    BEGIN
        SELECT COUNT(bbs_id) 
            INTO v_count
            FROM bbsboard
            WHERE bbs_id = p_bbs_id;

        IF v_count > 0 THEN
            UPDATE bbsboard
                SET is_enabled = '0'
                WHERE bbs_id = p_bbs_id;

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                COMMIT;
                v_count := 1;
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF;
        ELSE
            v_count := 0;
        END IF;

        p_result := v_count;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                p_result := -1;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Delete_Board;

    PROCEDURE Get_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_result            OUT     cv_result
    ) 
    AS
    BEGIN
        OPEN p_result FOR
            SELECT * FROM bbsboard WHERE bbs_id = p_bbs_id;
    END Get_Board;

    PROCEDURE Update_Board
    (
        p_bbs_id            IN      bbsboard.bbs_id%TYPE,
        p_bbs_name          IN      bbsboard.bbs_name%TYPE,
        p_bbs_type          IN      bbsboard.bbs_type%TYPE,
        p_is_enabled        IN      bbsboard.is_enabled%TYPE,
        p_result            OUT     INTEGER
    ) 
    AS
        v_count     INTEGER            := 0;
        v_errcode   INTEGER            := 0;
        v_errm      VARCHAR(4096)      := '';
    BEGIN
        SELECT COUNT(bbs_id) 
            INTO v_count
            FROM bbsboard
            WHERE bbs_id = p_bbs_id;

        IF v_count > 0 THEN
            UPDATE bbsboard
                SET 
                    bbs_name = p_bbs_name,
                    bbs_type = p_bbs_type,
                    is_enabled = p_is_enabled
                WHERE bbs_id = p_bbs_id;

            v_count := SQL%ROWCOUNT;

            IF v_count > 0 THEN
                COMMIT;
                v_count := 1;
            ELSE
                ROLLBACK;
                v_count := 0;
            END IF;
        ELSE
            v_count := 0;
        END IF;

        p_result := v_count;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                p_result := -1;
                v_errcode := SQLCODE;
                v_errm := SQLERRM(v_errcode);
                DBMS_OUTPUT.PUT_LINE('ERROR : ' || v_errcode || ' ' || v_errm);
                --RAISE_APPLICATION_ERROR(v_errcode, v_errm);
    END Update_Board;

END PKG_BBS_BOARD;

/

--------------------------------------------------------
--  DDL for Package Body PKG_BBS_MEMBER
--------------------------------------------------------

  CREATE OR REPLACE PACKAGE BODY "BBS"."PKG_BBS_MEMBER" 
AS

    PROCEDURE Get_By_Account_IDPW
    (
        p_account_id        IN      bbsmember.account_id%TYPE,
        p_account_pw        IN      bbsmember.account_pw%TYPE,
        p_result            OUT     cv_result
    ) AS
    BEGIN    
        OPEN p_result FOR
            SELECT * FROM bbsmember WHERE LOWER(account_id) = LOWER(p_account_id) AND account_pw = p_account_pw;        
    END Get_By_Account_IDPW;

END PKG_BBS_MEMBER;

/



REM INSERTING into BBS.BBSMEMBER
SET DEFINE OFF;
Insert into BBS.BBSMEMBER (MEMBER_ID,ACCOUNT_ID,ACCOUNT_PW,EMAIL,CREATED_TIME,IS_ENABLED,FULLNAME) values ('GUEST_MEMBER_ID','GUEST','guest','guest@test.test',to_date('23/08/31','RR/MM/DD'),'1','손님');

