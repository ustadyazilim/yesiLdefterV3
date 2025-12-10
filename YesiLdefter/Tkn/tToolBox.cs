using DevExpress.XtraBars.Alerter;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.LookAndFeel;
using DevExpress.XtraVerticalGrid;
using DevExpress.Utils;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

using Tkn_CreateObject;
using Tkn_CreateDatabase;
using Tkn_Events;
using Tkn_Forms;
using Tkn_Ftp;
using Tkn_Layout;
using Tkn_Registry;
using Tkn_Save;
using Tkn_SQLs;
using Tkn_TablesRead;
using Tkn_Variable;
using Tkn_IniFile;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraBars.Docking2010.Customization;
using System.Data.Sql;
using System.IO.Compression;
using Tkn_ExeUpdate;
using DevExpress.XtraCharts;
using System.Text.RegularExpressions;

namespace Tkn_ToolBox
{

    public class tToolBox: tBase
    {
        protected bool suppressManagerConnWarning = false;
        #region fileUpdates
        public void fileUpdatesChecked()
        {
            string lastMsFileUpdatesId = getMusteriFileUpdateIdList();
            //MessageBox.Show("1: " + lastMsFileUpdatesId);
            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();

            /// FileUpdate olup olmadığı sorgusu ise MainManagerV3 / UstadManagerV3 
            /// [dbo].[MsFileUpdates] üzerinden sorgulanıyor
            /// 
            string sql = sqls.Sql_MsFileUpdates(lastMsFileUpdatesId);
            //MessageBox.Show("2: " + sql);

            //if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref sql, "", "MsFileUpdates")) // test için kullan
            if (SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref sql, "", "MsFileUpdates")) // publish için kullan
            {
                //MessageBox.Show("fileUpdatesChecked - 2 " + sql);
                if (IsNotNull(ds))
                {
                    //MessageBox.Show("fileUpdatesChecked - 3 ");
                    v.con_NewFilesFound = true;
                    runMsFileUpdates(ds);
                }
            }
        }

        public void X64filesUpdates()
        {
            string lastMsFileUpdatesId = getMusteriFileUpdateIdList();

            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();

            /// FileUpdate olup olmadığı sorgusu ise MainManagerV3 / UstadManagerV3 
            /// [dbo].[MsFileUpdates] üzerinden sorgulanıyor
            /// 
            string sql = sqls.Sql_MsFileUpdates_X64();

            //if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref sql, "", "MsFileUpdates")) // test için kullan
            if (SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref sql, "", "MsFileUpdates")) // publish için kullan
            {
                //MessageBox.Show("fileUpdatesChecked - 2 " + sql);
                if (IsNotNull(ds))
                {
                    //MessageBox.Show("fileUpdatesChecked - 3 ");
                    runMsFileUpdates(ds);
                }
            }
        }

        private string getMusteriFileUpdateIdList()
        {
            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();
            string sql = "";
            string IdList = " 0 ";
            sql = sqls.Sql_FileUpdatesIdList();
            
            v.dBaseNo dBaseNo = v.dBaseNo.Project;
            if (v.active_DB.localDbUses)
                dBaseNo = v.dBaseNo.Local;

            //MessageBox.Show(" 0 : >" + dBaseNo.ToString() + "<"+ v.ENTER + sql);

            bool onay = SQL_Read_Execute(dBaseNo, ds, ref sql, "", "FileUpdates");

            if (onay)
            {
                if (IsNotNull(ds))
                {
                    int count = ds.Tables[0].Rows.Count;
                    // sadece maxID geliyor artık
                    if (count > 0)
                        IdList = ds.Tables[0].Rows[0]["MsFileUpdateId"].ToString();
                    else IdList = " 0 ";
                }
                else IdList = " 0 ";
            }
            else
            {
                IdList = " 0 ";
            }

            if (IdList == "")
                IdList = " 0 ";

            return IdList;
        }
        private void runMsFileUpdates(DataSet ds)
        {
            tExeUpdate exe = new tExeUpdate();

            bool onay = false;

            WaitFormOpen(Application.OpenForms[0] , "Yeni dosyalar indiriliyor ...");


            foreach (DataRow row in ds.Tables[0].Rows)
            {
                readMsFileUpdate(row);

                if (IsNotNull(v.tMsFileUpdate.pathName) == false)
                    v.tMsFileUpdate.pathName = v.tExeAbout.activePath;

                if (v.tMsFileUpdate.extension == "DIR" || v.tMsFileUpdate.extension == "Directory")
                {
                    onay = false;

                    //DosyaVarsaSil(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.packetName);
                    onay = checkedFileOnPc(v.tMsFileUpdate.packetName);

                    if (onay == false)
                    {
                        WaitFormOpen(Application.OpenForms[0], v.tMsFileUpdate.fileName + " klasörü indiriliyor...");

                        onay = ftpDownload(v.tMsFileUpdate.pathName, v.tMsFileUpdate.packetName); // MsFileUpdates te indirilmesi istenen dosyalar 

                        if (onay)
                        {
                            try
                            {
                                onay = false;

                                if (Directory.Exists(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName))
                                {
                                    // Klasörü ve içindeki her şeyi sil
                                    Directory.Delete(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName, true);
                                }

                                if (Directory.Exists(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName) == false)
                                {
                                    Directory.CreateDirectory(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName);
                                }

                                WaitFormOpen(Application.OpenForms[0], v.tMsFileUpdate.fileName + " ZIP dosyası açılıyor...");
                                ZipFile.ExtractToDirectory(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.packetName, v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName + "\\");// + "\\" + v.tMsFileUpdate.fileName);
                                onay = true;
                                AlertMessage(v.tMsFileUpdate.fileName, "ZIP dosyası başarıyla açıldı.");
                            }
                            catch (FileNotFoundException)
                            {
                                MessageBox.Show("ZIP dosyası bulunamadı.");
                            }
                            //catch (InvalidDataException)
                            //{
                            //    MessageBox.Show("ZIP dosyası bozuk veya geçersiz.");
                            //}
                            catch (Exception ex)
                            {
                                MessageBox.Show("Hata oluştu: " + ex.Message);
                            }
                        }
                    }
                }
                else if (v.tMsFileUpdate.extension.ToUpper() == "PACKET")
                {
                    //DosyaVarsaSil(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.packetName);
                    onay = checkedFileOnPc(v.tMsFileUpdate.packetName);

                    if (onay == false)
                    {
                        WaitFormOpen(Application.OpenForms[0], v.tMsFileUpdate.fileName + " dosya paketi indiriliyor...");

                        onay = ftpDownload(v.tMsFileUpdate.pathName, v.tMsFileUpdate.packetName); // MsFileUpdates te indirilmesi istenen dosyalar 

                        if (onay)
                        {
                            try
                            {
                                onay = false;

                                //if (Directory.Exists(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName))
                                //{
                                //    // Klasörü ve içindeki her şeyi sil
                                //    Directory.Delete(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName, true);
                                //}

                                //if (Directory.Exists(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName) == false)
                                //{
                                //    Directory.CreateDirectory(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.fileName);
                                //}

                                WaitFormOpen(Application.OpenForms[0], v.tMsFileUpdate.fileName + " ZIP dosyası açılıyor...");
                                //ZipFile.ExtractToDirectory(v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.packetName, v.tMsFileUpdate.pathName + "\\");
                                string zipPath = v.tMsFileUpdate.pathName + "\\" + v.tMsFileUpdate.packetName;
                                string extractPath = v.tMsFileUpdate.pathName;

                                using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Read))
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        string destinationPath = Path.Combine(extractPath, entry.FullName);

                                        // Eğer dosya varsa, üzerine yazmak için önce kaldırabiliriz
                                        if (File.Exists(destinationPath))
                                        {
                                            File.Delete(destinationPath);
                                        }

                                        entry.ExtractToFile(destinationPath, true); // 'true' değeri, üzerine yazmayı sağlar
                                    }
                                }

                                onay = true;
                                AlertMessage(v.tMsFileUpdate.fileName, "ZIP dosyası başarıyla açıldı.");
                            }
                            catch (FileNotFoundException)
                            {
                                MessageBox.Show("ZIP dosyası bulunamadı.");
                            }
                            //catch (InvalidDataException)
                            //{
                            //    MessageBox.Show("ZIP dosyası bozuk veya geçersiz.");
                            //}
                            catch (Exception ex)
                            {
                                MessageBox.Show("Hata oluştu: " + ex.Message);
                            }
                        }
                    }
                }
                else
                {
                    /// Bu dosya, daha önce bu pc de varmı kontrol et
                    ///
                    onay = checkedFileOnPc(v.tMsFileUpdate.fileName);

                    if (onay == false)
                    {
                        if (IsNotNull(v.tMsFileUpdate.pathName) == false)
                            v.tMsFileUpdate.pathName = v.tExeAbout.activePath;

                        WaitFormOpen(Application.OpenForms[0], "Dosya indiriliyor : " + v.tMsFileUpdate.fileName);

                        onay = ftpDownload(v.tMsFileUpdate.pathName, v.tMsFileUpdate.packetName); // MsFileUpdates te indirilmesi istenen dosyalar 
                        if (onay)
                        {
                            onay = false;
                            WaitFormOpen(Application.OpenForms[0], "ZIP dosya açılıyor : " + v.tMsFileUpdate.fileName);
                            onay = exe.ExtractFile(v.tMsFileUpdate.pathName, v.tMsFileUpdate.packetName);
                        }
                        // activeFileName   = v.tExeAbout.activeExeName
                        // extension        = 
                        // oldVersionNo     = v.tExeAbout.activeVersionNo
                        // packetName       = v.tExeAbout.ftpPacketName
                        if (onay)
                            onay = exe.fileNameChange(v.tMsFileUpdate.pathName, v.tMsFileUpdate.fileName, v.tMsFileUpdate.extension, v.tMsFileUpdate.versionNo, v.tMsFileUpdate.packetName);
                    }
                }

                // FileUpdates tablosuna işle
                if (onay)
                    insertFileUpdates();
            }

            v.IsWaitOpen = false;
            WaitFormClose();
        }
        private bool checkedFileOnPc(string fileName)
        {
            bool onay = false;

            try
            {
                string fName = v.tExeAbout.activePath + "\\" + fileName;
                if (File.Exists(@"" + fName))
                {
                    onay = true;
                }
            }
            catch
            { }

            return onay;
        }

        private void readMsFileUpdate(DataRow msFileUpdatesRow)
        {
            v.tMsFileUpdate.Clear();

            v.tMsFileUpdate.id = (int)msFileUpdatesRow["Id"];
            v.tMsFileUpdate.fileName = msFileUpdatesRow["FileName"].ToString();
            v.tMsFileUpdate.extension = msFileUpdatesRow["Extension"].ToString();
            v.tMsFileUpdate.versionNo = msFileUpdatesRow["VersionNo"].ToString();
            v.tMsFileUpdate.packetName = msFileUpdatesRow["PacketName"].ToString();
            v.tMsFileUpdate.pathName = msFileUpdatesRow["PathName"].ToString();
            v.tMsFileUpdate.about = msFileUpdatesRow["About"].ToString();
        }
        private void insertFileUpdates()
        {
            /// Sql execute sırasında script kontrolünden geçmeyecek
            ///
            v.con_CreateScriptPacket = true;

            DataSet ds = new DataSet();
            string sql = "";
            sql = Sql_FileUpdatesInsert();
            try
            {
                //MessageBox.Show(sql);
                SQL_Read_Execute(v.dBaseNo.Project, ds, ref sql, "", "FileUpdates");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : FileUpdates : " + ex.Message);
                throw;
            }
            v.con_CreateScriptPacket = false;
        }

        private string Sql_FileUpdatesInsert()
        {
            // müşteri database
            string networkKey = v.tComputer.Network_MACAddress;
            string pcName = v.tComputer.PcName;

            // Müşteri database üzerinde çalıştırılan update lerin kaydı
            string cumle =
            @" if ( Select count(*) ADET from [dbo].[FileUpdates] where 0 = 0 
                    and [PcName] = '" + pcName + @"' 
                    and [NetworkMacAddress] = '" + networkKey + @"'
                    and [MsFileUpdateId] = " + v.tMsFileUpdate.id.ToString() + @" ) = 0 
        begin
            INSERT INTO [dbo].[FileUpdates]
           ([IsActive]
           ,[MsFileUpdateId]
           ,[UpdateDate]
           ,[FileName]
           ,[Extension]
           ,[VersionNo]
           ,[PacketName]
           ,[PathName]
           ,[About]
           ,[PcName]
           ,[NetworkMacAddress])
            VALUES
           ( 1," + v.tMsFileUpdate.id.ToString() +
            ", getdate() " +
            ",'" + v.tMsFileUpdate.fileName + "'" +
            ",'" + v.tMsFileUpdate.extension + "'" +
            ",'" + v.tMsFileUpdate.versionNo + "'" +
            ",'" + v.tMsFileUpdate.packetName + "'" +
            ",'" + v.tMsFileUpdate.pathName + "'" +
            ",'" + v.tMsFileUpdate.about + "'" +
            ",'" + pcName + "'" +
            ",'" + networkKey + "'" +
           @"); 
        end ";
            return cumle;
        }

        #endregion fileUpdates

        #region DataUpdates


        public void dataUpdates()
        {
            // UstadMtsk ise
            if ((v.active_DB.localDbUses == false) &&
                (v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.UstadMtsk))
            {
                Task task1 = new Task(() =>
                {
                    dataUpdatesUstadMtsk();
                });
                task1.Start();    
            }
        }

        public async Task dataUpdatesUstadMtsk()
        { 
            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();

            string MtskSaatTeorikSql = "";
            string MtskSaatUygulamaSql = "";
            string MtskUcretSinavSql = "";
            string MtskUcretTeorikSql = "";
            string MtskUcretUygulamaSql = "";
            string insertSql = "";
            string insertSqlMaster = @"  declare @FIRM_ID int 
  Set @FIRM_ID = :FIRM_ID 
  declare @TarihId int = 0
";

            /// 1. GecerliTarih çalıştırılıyor
            /// 2. Saatler çalıştırılıyor
            /// 3. Ücretler çalıştırılıyor


            /// 1. Hazırlık
            /// 
            insertSql = insertSqlMaster;

            /// HubGecerliTarih'leri okuyup GecerliTarih tablosuna insert eden sql
            /// 
            string selectSql = sqls.getGecerliTarihSql();
            /// Sadece GecerlilikTarihi ni insert et
            if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref selectSql, "", "GecerliTarih"))
            {
                if (IsNotNull(ds))
                {
                    /// GecerliTarih tablo insert satırları
                    /// 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        insertSql += row[0].ToString() + v.ENTER;
                    }
                }
            }
            /// hazırlanan insert satılarını çalıştır
            /// 
            if (IsNotNull(insertSql))
            {
                TableRemove(ds);

                if (SQL_Read_Execute(v.dBaseNo.Project, ds, ref insertSql, "", "GecerliTarih"))
                {
                    //
                }
            }


            /// 2. Hazırlık
            /// 
            insertSql = insertSqlMaster;

            /// HubMtskSaatUygulama > MtskSaatTeorik
            /// 
            insertSql += sqls.getGecerlilikTarihiIdSql(21101);
            MtskSaatTeorikSql = sqls.getMtskSaatTeorikSql();

            TableRemove(ds);

            if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref MtskSaatTeorikSql, "", "MtskSaatTeorik"))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    insertSql += row[0].ToString() + v.ENTER;
                }
            }

            /// HubMtskSaatUygulama > MtskSaatUygulama
            /// 
            insertSql += sqls.getGecerlilikTarihiIdSql(21102);
            MtskSaatUygulamaSql = sqls.getMtskSaatUygulamaSql();

            TableRemove(ds);

            if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref MtskSaatUygulamaSql, "", "MtskSaatUygulama"))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    insertSql += row[0].ToString() + v.ENTER;
                }
            }

            /// hazırlanan insert satılarını çalıştır
            /// 
            if (IsNotNull(insertSql))
            {
                TableRemove(ds);

                if (SQL_Read_Execute(v.dBaseNo.Project, ds, ref insertSql, "", "Saatler"))
                {
                    //
                }
            }


            /// 3. Hazırlık
            /// 
            insertSql = insertSqlMaster;

            /// HubMtskUcretSinav > MtskUcretSinav
            ///
            insertSql += sqls.getGecerlilikTarihiIdSql(21103);
            MtskUcretSinavSql = sqls.getMtskUcretSinavSql();

            TableRemove(ds);

            if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref MtskUcretSinavSql, "", "MtskUcretSinav"))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    insertSql += row[0].ToString() + v.ENTER;
                }
            }

            /// HubMtskUcretTeorik > MtskUcretTeorik
            /// 
            insertSql += sqls.getGecerlilikTarihiIdSql(21104);
            MtskUcretTeorikSql = sqls.getMtskUcretTeorikSql();

            TableRemove(ds);

            if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref MtskUcretTeorikSql, "", "MtskUcretTeorik"))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    insertSql += row[0].ToString() + v.ENTER;
                }
            }

            /// HubMtskUcretUygulama > MtskUcretUygulama
            insertSql += sqls.getGecerlilikTarihiIdSql(21104);
            MtskUcretUygulamaSql = sqls.getMtskUcretUygulamaSql();

            TableRemove(ds);

            if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref MtskUcretUygulamaSql, "", "MtskUcretUygulama"))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    insertSql += row[0].ToString() + v.ENTER;
                }
            }


            /// hazırlanan insert satılarını çalıştır
            /// 
            if (IsNotNull(insertSql))
            {
                TableRemove(ds);

                if (SQL_Read_Execute(v.dBaseNo.Project, ds, ref insertSql, "", "Ucretler"))
                {
                    //
                }
            }

        }

        public void dataUpdates_IPTAL()
        {
            /// Hub ile başlayan tablolar bizim serverlerde tuttuğumuz ortak paylaşımlı tablolardır.
            /// Biz bu Hubxxxx tablolara bir kayıt girince otomatik olarak müşteri database lerde olmasını sağladığımız datalardır
            /// Ücretler, saatler vb ortak olan ve herkesi ilgilendiren datalardır
            /// 

            tSQLs sqls = new tSQLs();
            DataSet dsLastIds = new DataSet();
            string lastIdSql = sqls.getDataUpdatesLastIdSql();
            
            if (SQL_Read_Execute(v.dBaseNo.Manager, dsLastIds, ref lastIdSql, "", "LastIds"))
            {
                if (IsNotNull(dsLastIds))
                {
                    
                    string newRowsSql = sqls.getHubDataUpdatesFindNewRowsSql(dsLastIds);
                    DataSet newHubDataRows = new DataSet();

                    if (SQL_Read_Execute(v.dBaseNo.Manager, newHubDataRows, ref newRowsSql, "", "newRowsSql"))
                    {
                        if (IsNotNull(newHubDataRows))
                        {
                            ////.... insert yapılacatı kaldı , çünkü FirmId faktöündn dolayı olmadı
                        }

                    }
                    
                }
            }
        }

        #endregion DataUpdates

        #region *Database İşlemleri

        #region dbUpdatesChecked
        public bool dbUpdatesChecked()
        {
            /// Burdaki Id Project Database den [dbo].[DbUpdates] tablosundan okunuyor
            /// yani Müşteri database den
            /// 
            string lastMsDbUpdatesId = 
                getMusteriDbUpdateIdList();
            bool onay = false;
            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();

            //MessageBox.Show("dbUpdatesChecked - 1 ");

            /// Update olup olmadığı sorgusu ise MainManagerV3 / UstadManagerV3 
            /// [dbo].[MsDbUpdates] üzerinden sorgulanıyor
            /// 
            string sql = sqls.Sql_MsDbUpdates(lastMsDbUpdatesId);

            /// Kodlama ve Test aşamasında kullan
            //if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref sql, "", "MsDbUpdates")) 
            /// publish sırasında kullan
            if (SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref sql, "", "MsDbUpdates")) 
            {
                //MessageBox.Show("dbUpdatesChecked - 2 " + sql);
                if (IsNotNull(ds))
                {
                    //MessageBox.Show("dbUpdatesChecked - 3 ");
                    runMsDbUpdates(ds);
                    onay = true;
                }
            }
            return onay;
        }
        private string getMusteriDbUpdateIdList()
        {
            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();
            string sql = "";
            string IdList = " 0 ";
            sql = sqls.Sql_DbUpdatesIdList();

            v.dBaseNo dBaseNo = v.dBaseNo.Project;
            if (v.active_DB.localDbUses) 
                dBaseNo = v.dBaseNo.Local;

            bool onay = SQL_Read_Execute(dBaseNo, ds, ref sql, "", "DbUpdates");

            if (onay)
            {
                if (IsNotNull(ds))
                {
                    int count = ds.Tables[0].Rows.Count;
                    //for (int i = 0; i < count; i++)
                    //{
                    //    IdList = IdList + ", " + ds.Tables[0].Rows[i]["MsDbUpdateId"].ToString();
                    //}
                    // sadece maxID geliyor artık
                    if (count > 0)
                        IdList = ds.Tables[0].Rows[0]["MsDbUpdateId"].ToString();
                    else IdList = " 0 ";
                }
                else IdList = " 0 ";
            }
            else
            {
                IdList = " 0 ";
            }
            
            if (IdList == "")
                IdList = " 0 ";

            return IdList;            
        }
        public void runMsDbUpdates(DataSet ds)
        {
            Int16 typeId = 0;
            bool onay = true;

            //MessageBox.Show("readMsDbUpdatesList - 1 ");

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                typeId = Convert.ToInt16(row["UpdateTypeId"].ToString());

                readMsDbUpdate(row);

                if (typeId == 11) onay = runDbUpdateData();
                //if (typeId == 12)
                if (typeId == 14) onay = runSqlScript();
                if (typeId == 21) onay = runDbUpdateTableAdd_();
                if (typeId == 22) onay = runDbUpdateFieldAdd();
                if (typeId == 23) onay = runDbUpdateFieldUpdate();
                //if (typeId == 24)
                if (typeId == 31) onay = runDbUpdateTriggerAdd();
                //if (typeId == 32) 
                if (typeId == 41) onay = runDbUpdateProcedureAdd();
                //if (typeId == 42) 
                //if (typeId == 51) 
                //if (typeId == 52) 

                /// hata oluşmuş ise işlemi bırak
                ///
                if (onay == false) break;
            }

            /*
  [Lkp].[MsDbUpdatesUpdateType] (Id, UpdateType)
  ( 11,    N'Data  update - ManagerDb den table datasını al'),
  ( 12,    N'Rapor update - ManagerDb den rapor datasını al'),
  ( 13,    N'Data transfer işlemi'),
  ( 14,    N'SQL script çalıştır'),

  ( 21,    N'Table add'),
  ( 22,    N'Field add'),
  ( 23,    N'Field update'),
  ( 24,    N'Field rename'),

  ( 31,    N'Trigger add'),
  ( 32,    N'Trigger update'),
  ( 41,    N'Procedure add'),
  ( 42,    N'Procedure update'),
  ( 51,    N'Function add'),
  ( 52,    N'Function update')
            */
        }
        private void readMsDbUpdate(DataRow msDbUpdatesRow)
        {
            v.tMsDbUpdate.Clear();

            v.tMsDbUpdate.id = (int)msDbUpdatesRow["Id"];
            v.tMsDbUpdate.sectorTypeId = (Int16)msDbUpdatesRow["SectorTypeId"];
            v.tMsDbUpdate.updateTypeId = (Int16)msDbUpdatesRow["UpdateTypeId"];

            v.tMsDbUpdate.dBaseNoTypeId = (Int16)msDbUpdatesRow["DBaseNoTypeId"];
            v.tMsDbUpdate.schemaName = msDbUpdatesRow["SchemaName"].ToString();
            v.tMsDbUpdate.tableName = msDbUpdatesRow["TableName"].ToString();
            v.tMsDbUpdate.fieldName = msDbUpdatesRow["FieldName"].ToString();
            v.tMsDbUpdate.fieldTypeId = myInt16(msDbUpdatesRow["FieldTypeId"].ToString());
            v.tMsDbUpdate.fieldLength = Set(msDbUpdatesRow["FieldLength"].ToString(), "", "");
            v.tMsDbUpdate.fieldNotNull = myBool(msDbUpdatesRow["FieldNotNull"].ToString());
            v.tMsDbUpdate.sqlScript = msDbUpdatesRow["SqlScript"].ToString();
        }

        // 11,    Data  update - ManagerDb den table datasını al
        private bool runDbUpdateData()
        {
            // Data Update :
            // publishManager de olan bir tablonun tüm kayıtlarını oku ve 
            // müşterinin database ne insert et
            // Örnek : MtskSablonTeorik, MtskSablonUygulama vb..

            vScripts scripts = new vScripts();
            scripts.SourceDBaseName = Find_dBLongName(Convert.ToString((byte)v.dBaseNo.publishManager));
            scripts.SchemaName = v.tMsDbUpdate.schemaName;
            scripts.SourceTableName = v.tMsDbUpdate.tableName;
            scripts.IdentityInsertOnOff = true;

            // insert cumlesi hazırlansın
            string cumle = preparingInsertScript(scripts);
            // hangi Database yazılacak 
            v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
            // hazırlanan cumleyi müşteri database üzerinde çalıştıralım
            bool onay = runScript(dBaseNo, cumle);

            // Müşteri database nin üzerinde yapılan işlemler DBUpdates tablosuna kaydedilecek
            if (onay)
            {
                insertDbUpdates();
            }
            return onay;
        }

        // 12,    Rapor update - ManagerDb den rapor datasını al
        private void runDbUpdateReport()
        {

        }
        // 14,    SQL script çalıştır
        private bool runSqlScript()
        {
            // hangi Database yazılacak 
            v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
            string cumle = v.tMsDbUpdate.sqlScript;

            cumle = Str_AntiCheck(cumle);

            // hazırlanan cumleyi müşteri database üzerinde çalıştıralım
            bool onay = runScript(dBaseNo, cumle);

            // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
            if (onay)
            {
                insertDbUpdates();
            }

            return onay;
        }

        // 21,    Table add
        private bool runDbUpdateTableAdd_()
        {
            vTable vt = new vTable();
            vt.DBaseNo = v.active_DB.projectDBaseNo;
            vt.SchemasCode = v.tMsDbUpdate.schemaName;
            vt.TableName = v.tMsDbUpdate.tableName;
            vt.ParentTable = "";
            vt.SqlScript = "";

            bool onay = runDbUpdateTableAdd(vt, 0);
            //string notExistsSql = preparingTableIFNotExists(schemaName, tableName);
            return onay;
        }

        public bool runDbUpdateTableAdd(vTable vt, Int16 sectorTypeId)
        {
            tDatabase db = new tDatabase();
            tSQLs sql = new tSQLs();

            bool onay = false;

            /// tablonun scriptini temin edelim

            if (sectorTypeId == 0)
                sectorTypeId = v.SP_Firm_SectorTypeId;

            string tSql = sql.Sql_MsProjectTables(vt.TableName, sectorTypeId);

            DataSet ds = new DataSet();

            if (SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref tSql, "", "tableScripts"))
            {
                if (IsNotNull(ds))
                {
                    vt.SqlScript = ds.Tables[0].Rows[0]["SqlScript"].ToString();

                    /// script elimizde artık create işlemine geçelim 
                    if (IsNotNull(vt.SqlScript))
                        onay = db.preparingCreateTable(vt);

                    // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
                    if (onay)
                    {
                        if (v.tMsDbUpdate.id > 0)
                            insertDbUpdates();
                    }
                }
                else
                {
                    if (IsNotNull(v.tMsDbUpdate.sqlScript))
                    {
                        // MsDbUpdates üzerindeki Sql Scripti al
                        v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
                        string databaseName = Find_dBLongName(Convert.ToString((byte)dBaseNo));
                        vt.SchemasCode = v.tMsDbUpdate.schemaName;
                        vt.DBaseNo = dBaseNo;
                        vt.DBaseName = databaseName;
                        vt.TableName = v.tMsDbUpdate.tableName;
                        vt.SqlScript = v.tMsDbUpdate.sqlScript;

                        onay = db.preparingCreateTable(vt);

                        // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
                        if (onay)
                        {
                            if (v.tMsDbUpdate.id > 0)
                                insertDbUpdates();
                        }
                    }
                    else
                    {
                        MessageBox.Show("DİKKAT : Yeni oluşturmak istediğiniz tabloaya ait script bulunamdı." + v.ENTER2 +
                            "TableName : " + vt.TableName + v.ENTER +
                            "SectorTypeId : " + sectorTypeId.ToString()
                            );
                    }
                }
            }
            else
            {
                MessageBox.Show("DİKKAT : Yeni oluşturmak istediğiniz tabloaya ait script bulunamdı." + v.ENTER2 +
                    "TableName : " + vt.TableName + v.ENTER +
                    "SectorTypeId : " + sectorTypeId.ToString()
                    ) ;
            }
            return onay;
        }

        public string preparingTableIFNotExists(string schemaName, string tableName)
        {
            return 
              " IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + schemaName + "].[" + tableName + "]') AND type in (N'U')) "
            + " begin "
            + " print 'create_table' "
            + " end ";
        }
        // 22,    Field add
        private bool runDbUpdateFieldAdd()
        {
            // hangi Database yazılacak 
            v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
            string databaseName = Find_dBLongName(Convert.ToString((byte)dBaseNo));
            string tNull = "null";
            if (v.tMsDbUpdate.fieldNotNull)
                tNull = " not null";

            // fieldAdd cumlesini hazırlansın
            string cumle = preparingTableFieldAdd(
                databaseName,
                v.tMsDbUpdate.schemaName,
                v.tMsDbUpdate.tableName,
                v.tMsDbUpdate.fieldName,
                getFieldTypeName(v.tMsDbUpdate.fieldTypeId),
                v.tMsDbUpdate.fieldLength,
                tNull
                ); 
            
            // hazırlanan cumleyi müşteri database üzerinde çalıştıralım
            bool onay = runScript(dBaseNo, cumle);

            // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
            if (onay)
            {
                insertDbUpdates();
            }
            return onay;
        }
        // 23,    Field update
        private bool runDbUpdateFieldUpdate()
        {
            //ALTER TABLE MS_TABLES_IP ALTER COLUMN EXTERNAL_IP_CODE VARCHAR(50) NULL

            // hangi Database yazılacak 
            v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
            string databaseName = Find_dBLongName(Convert.ToString((byte)dBaseNo));
            string tNull = "null";
            if (v.tMsDbUpdate.fieldNotNull)
                tNull = " not null";

            // fieldAdd cumlesini hazırlansın
            string cumle = preparingTableFieldUpdate(
                databaseName,
                v.tMsDbUpdate.schemaName,
                v.tMsDbUpdate.tableName,
                v.tMsDbUpdate.fieldName,
                getFieldTypeName(v.tMsDbUpdate.fieldTypeId),
                v.tMsDbUpdate.fieldTypeId,
                v.tMsDbUpdate.fieldLength,
                tNull
                );

            // hazırlanan cumleyi müşteri database üzerinde çalıştıralım
            bool onay = runScript(dBaseNo, cumle);

            // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
            if (onay)
            {
                insertDbUpdates();
            }
            return onay;
        }
        // 24,    Field rename
        private void runDbUpdateFieldRename()
        {

        }
        // 31,    Trigger add
        private bool runDbUpdateTriggerAdd()
        {
            v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
            string cumle1 = v.tMsDbUpdate.sqlScript;
            string cumle2 = v.tMsDbUpdate.sqlScript;

            /// cumle1 ve cumle2 aynı sqllerdir
            /// drop kontrolü yapılıyor, yok ise önce drop sqli hazırla ve uygula
            /// 
            if (cumle1.IndexOf("drop") == -1)
            {
                /// trigger mevcut ise önce sil
                /// 
                if (cumle1.IndexOf("sys.triggers") == -1)
                {
                    cumle1 = @"
                    if ( select count(object_id) as ADET from sys.triggers where name = '" + v.tMsDbUpdate.tableName + @"') = 1 
                    begin
                        DROP TRIGGER [" + v.tMsDbUpdate.schemaName + @"].[" + v.tMsDbUpdate.tableName + @"] 
                    end ";

                    runScript(dBaseNo, cumle1);
                }
            }
            // create cumlesi            
            cumle2 = Str_AntiCheck(cumle2);
            // hazırlanan cumleyi müşteri database üzerinde çalıştıralım
            bool onay = runScript(dBaseNo, cumle2);

            // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
            if (onay)
            {
                insertDbUpdates();
            }
            return onay;
        }
        // 32,    Trigger update
        private void runDbUpdateTriggerUpdate()
        {
            //
        }
        // 41,    Procedure add
        private bool runDbUpdateProcedureAdd()
        {
            v.dBaseNo dBaseNo = getDBaseNo(v.tMsDbUpdate.dBaseNoTypeId.ToString());
            string cumle1 = v.tMsDbUpdate.sqlScript;
            string cumle2 = v.tMsDbUpdate.sqlScript;

            /// cumle1 ve cumle2 aynı sqllerdir
            /// drop kontrolü yapılıyor, yok ise önce drop sqli hazırla ve uygula
            /// 
            if (cumle1.IndexOf("drop") == -1)
            {
                /// procedure mevcut ise önce sil
                /// 
                if (cumle1.IndexOf("sys.procedures") == -1)
                {
                    cumle1 = @"
                    if ( select count(object_id) as ADET from sys.procedures where name = '" + v.tMsDbUpdate.tableName + @"') = 1 
                    begin
                        DROP PROCEDURE [" + v.tMsDbUpdate.schemaName + @"].[" + v.tMsDbUpdate.tableName + @"] 
                    end ";

                    runScript(dBaseNo, cumle1);
                }
            }
            // create cumlesi            
            cumle2 = Str_AntiCheck(cumle2);
            // hazırlanan cumleyi müşteri database üzerinde çalıştıralım
            bool onay = runScript(dBaseNo, cumle2);

            // Müşteri database nin üzerinde yapılan işlemleri DBUpdates tablosuna kaydedilecek
            if (onay)
            {
                insertDbUpdates();
            }
            return onay;
        }
        // 42,    Procedure update
        private void runDbUpdateProcedureUpdate()
        {
            //
        }
        // 51,    Function add
        private void runDbUpdateFunctionAdd()
        {

        }
        // 52,    Function update
        private void runDbUpdateFunctionUpdate()
        {

        }

        private void insertDbUpdates()
        {
            /// Sql execute sırasında script kontrolünden geçmeyecek
            ///
            v.con_CreateScriptPacket = true;

            DataSet ds = new DataSet();
            string sql = "";
            sql = Sql_DbUpdatesInsert();
            try
            {
                SQL_Read_Execute(v.dBaseNo.Project, ds, ref sql, "", "DbUpdates");
            }
            catch (Exception)
            {
                throw;
            }
            v.con_CreateScriptPacket = false;
        }

        private string Sql_DbUpdatesInsert()
        {
            // Müşteri database üzerinde çalıştırılan update lerin kaydı
            string cumle =
            @" if ( Select count(*) ADET from [dbo].[DbUpdates] where 0 = 0 
                    and [MsDbUpdateId] = " + v.tMsDbUpdate.id.ToString() + @" ) = 0 
        begin
            INSERT INTO [dbo].[DbUpdates]
           ([IsActive]
           ,[MsDbUpdateId]
           ,[UpdateDate]
           ,[SectorTypeId]
           ,[UpdateTypeId]
           ,[DBaseNoTypeId]
           ,[SchemaName]
           ,[TableName]
           ,[FieldName]
           ,[FieldTypeId]
           ,[FieldLength]
           ,[FieldNotNull]
           ,[About]
           ,[SqlScript])
            VALUES
           ( 1," + v.tMsDbUpdate.id.ToString() +
            ", getdate() " +
            ", " + v.tMsDbUpdate.sectorTypeId.ToString() +
            ", " + v.tMsDbUpdate.updateTypeId.ToString() +
            ", " + v.tMsDbUpdate.dBaseNoTypeId.ToString() +
            ",'" + v.tMsDbUpdate.schemaName + "'" +
            ",'" + v.tMsDbUpdate.tableName + "'" +
            ",'" + v.tMsDbUpdate.fieldName + "'" +
            ", " + v.tMsDbUpdate.fieldTypeId.ToString() +
            ",'" + v.tMsDbUpdate.fieldLength + "'" +
            ", " + myBoolMsSql(v.tMsDbUpdate.fieldNotNull.ToString()) +
            ",'" + v.tMsDbUpdate.about + "'" +
            ",'" + v.tMsDbUpdate.sqlScript + "'" +
           @"); 
        end "; 
            return cumle;
        }

        public string preparingInsertScript(vScripts scripts)
        {
            DataSet dsQuery = new DataSet();
            string cumleDelete = " Delete   From [{0}].[{1}] ";
            string cumleSelect = " Select * From [{0}].[{1}] ";

            string databaseName = scripts.SourceDBaseName;
            string schemaName = scripts.SchemaName;
            string tableIPCode = scripts.TableIPCode;
            string tableName = scripts.SourceTableName;
            string where = scripts.Where;
            bool identityInsertOnOff = scripts.IdentityInsertOnOff;

            string cumle = "";
            string tSql = "";
            string myProp = "";

            if (schemaName == "") schemaName = "dbo";

            if (where != "")
            {
                cumleDelete = " Delete   From [{0}].[{1}] Where {2} ";
                cumle = string.Format(cumleDelete, schemaName, tableName, where) + v.ENTER2;

                cumleSelect = " Select * From [{0}].[{1}] Where {2} ";
                tSql = string.Format(cumleSelect, schemaName, tableName, where);
            }
            else
            {
                cumle = string.Format(cumleDelete, schemaName, tableName) + v.ENTER2;
                tSql = string.Format(cumleSelect, schemaName, tableName);
            }

            myProp = preparingMyProp(databaseName, schemaName, tableName, tableIPCode, tSql);

            dsQuery.Namespace = myProp;

            Data_Read_Execute(null, dsQuery, ref tSql, tableName, null);

            if (IsNotNull(dsQuery))
            {
                vTable vt = new vTable();
                preparing_vTable(null, myProp, vt, dsQuery.Tables.Count);
                vt.TableName = tableName;
                vt.IdentityInsertOnOff = identityInsertOnOff;

                tSave sv = new tSave();
                cumle = cumle + sv.Insert_Script_Multi(dsQuery, vt); 
            }

            dsQuery.Dispose();

            return cumle;
        }

        private string preparingTableFieldAdd(
            string databaseName,
            string schemaName,
            string tableName, 
            string fieldName, 
            string fieldTypeName, 
            string fieldLength, 
            string fieldNull)
        {
            tSQLs sqls = new tSQLs();

            if (schemaName == "") schemaName = "dbo";
            if (fieldNull == "") fieldNull = " null ";
            if (fieldLength != "") 
                if (fieldLength.IndexOf("(") == -1)
                    fieldLength = "(" + fieldLength + ")";

            string findSql = sqls.SQL_FieldFind(databaseName, tableName, fieldName);

            string Sql =
            @" IF not EXISTS ( " + findSql  + @" )
               begin
                   ALTER TABLE " + schemaName + "." + tableName + @" ADD " + fieldName + @" " + fieldTypeName + fieldLength + @" " + fieldNull + @" 
               end
            ";

            return Sql;
        }

        private string preparingTableFieldUpdate(
            string databaseName,
            string schemaName,
            string tableName,
            string fieldName,
            string fieldTypeName,
            Int16 fieldTypeId,
            string fieldLength,
            string fieldNull)
        {
            tSQLs sqls = new tSQLs();

            if (schemaName == "") schemaName = "dbo";
            if (fieldNull == "") fieldNull = " null ";
                        
            string findSql = sqls.SQL_FieldNameAndTypeFind(databaseName, tableName, fieldName, fieldTypeId, fieldLength);

            if ((fieldLength.IndexOf("(") == -1) && (fieldLength != ""))
                 fieldLength = " (" + fieldLength + ") ";

            string Sql = // IF not EXISTS
            @" IF EXISTS ( " + findSql + @" )
               begin
                   ALTER TABLE " + schemaName + "." + tableName + @" ALTER COLUMN " + fieldName + @" " + fieldTypeName + fieldLength + @" " + fieldNull + @" 
               end
            ";

            return Sql;
        }

        public string preparingMyProp(string databaseName, string schemas, string tableName, string tableIPCode, string sql)
        {
            
            v.dBaseNo dBaseNo = getDBaseNo(databaseName);
            
            string myProp = "";
            MyProperties_Set(ref myProp, "DBaseNo", Convert.ToString((byte)dBaseNo));
            MyProperties_Set(ref myProp, "DBaseName", databaseName);
            MyProperties_Set(ref myProp, "SchemasCode", schemas);
            MyProperties_Set(ref myProp, "TableName", tableName);
            MyProperties_Set(ref myProp, "TableIPCode", tableIPCode);
            MyProperties_Set(ref myProp, "SqlFirst", sql);
            MyProperties_Set(ref myProp, "SqlSecond", "null");
            MyProperties_Set(ref myProp, "TableType", "1");
            MyProperties_Set(ref myProp, "Cargo", "data");
            MyProperties_Set(ref myProp, "KeyFName", "");

            return myProp;
        }

        public bool preparingCreateTable(v.dBaseNo dBaseNo, string schemasCode, string tableName, Int16 sectorTypeId)
        {
            bool onay = false;
            tDatabase db = new tDatabase();

            vTable vt = new vTable();
            vt.DBaseNo = dBaseNo; 
            vt.SchemasCode = schemasCode;
            vt.TableName = tableName;
            vt.ParentTable = "";
            vt.SqlScript = "";

            onay = db.tTableFind(vt);

            if (onay == false)
            {
                onay = runDbUpdateTableAdd(vt, sectorTypeId);
            }

            Application.DoEvents();

            return onay;
        }

        #endregion dbUpdatesChecked

        #region Db_Open

        /// <summary>
        /// MSSQL Bağlantısı
        /// </summary>
        public Boolean Db_Open(SqlConnection VTbaglanti)
        {
            bool onay = false;

            // Guard: connection object must not be null
            if (VTbaglanti == null)
            {
                v.SP_ConnBool_Manager = false;
                System.Diagnostics.Debug.WriteLine("Db_Open called with null SqlConnection.");
                return false;
            }

            // Guard: connection string must be present
            if (string.IsNullOrWhiteSpace(VTbaglanti.ConnectionString))
            {
                v.SP_ConnBool_Manager = false;
                System.Diagnostics.Debug.WriteLine("Db_Open called with empty ConnectionString.");
                return false;
            }

            // Capture state safely after null check
            ConnectionState state;
            try
            {
                state = VTbaglanti.State;
            }
            catch (NullReferenceException)
            {
                v.SP_ConnBool_Manager = false;
                System.Diagnostics.Debug.WriteLine("Db_Open: SqlConnection became null while checking State.");
                return false;
            }

            #region Closed ise
            if (state == ConnectionState.Closed)
            {
                byte i = 0;

                //WaitFormOpen(v.mainForm, "MsSQL " + v.Wait_Desc_DBBaglanti);

                try
                {
                    VTbaglanti.Open();
                    onay = true;
                    v.SP_ConnBool_Manager = true;

                    if (v.SP_OpenApplication == false)
                    {
                        //Thread.Sleep(500);
                        //SplashScreenManager.CloseForm(false);
                    }
                    else
                    {
                        //WaitFormOpen(v.mainForm, v.Wait_Desc_ProgramYukDevam);
                    }
                }
                catch (Exception e)
                {
                    string conn = VTbaglanti.ConnectionString;
                    //  Data Source = 111.222.333.444; Initial Catalog = xxxxxxx; User ID = sa; Password = *****; MultipleActiveResultSets = True 
                    int i1 = conn.IndexOf("Password");
                    int i2 = conn.IndexOf("MultipleActiveResultSets");
                    conn = conn.Remove(i1 + 10, i2 - i1 - 10); // password ü sil

                    MessageBox.Show("HATA : MSSQL Database bağlantısı açılmadı ... " + v.ENTER2 +
                        "Connection : " + conn + v.ENTER2 +
                        "*  Bunun çeşitli sebepleri olabilir." + v.ENTER2 +
                        "1. Database in bulunduğu bilgisayar ( SERVER ) kapalı olabilir ..." + v.ENTER +
                        "2. MSSQL Server kapalı olabilir ..." + v.ENTER +
                        "3. SQL Server Authentication ( Windows Authentication yerine [sa] ) ayarlı olmayabilir." + v.ENTER +
                        "4. Network bağlantınızda sorun olabilir ..." + v.ENTER +
                        "5. Database bağlantı tanımlarında sorun olabilir ..." + v.ENTER2 +
                        "   Bu nedenle size sorulan soruları kontrol edin, yine olmaz ise yardım isteyin ..."
                        + v.ENTER2
                        + e.Message.ToString());

                    if (VTbaglanti == v.active_DB.masterMSSQLConn) i = 1;
                    if (VTbaglanti == v.active_DB.managerMSSQLConn) i = 2;
                    if (VTbaglanti == v.active_DB.ustadCrmMSSQLConn) i = 3;
                    if (VTbaglanti == v.active_DB.projectMSSQLConn) i = 4;

                    //Xml_.Database_Ondegerleri_Yenile();

                    try
                    {
                        if (i == 1) VTbaglanti = new SqlConnection(v.active_DB.masterConnectionText);
                        if (i == 2) VTbaglanti = new SqlConnection(v.active_DB.managerConnectionText);
                        if (i == 3) VTbaglanti = new SqlConnection(v.active_DB.ustadCrmConnectionText);
                        if (i == 4) VTbaglanti = new SqlConnection(v.active_DB.projectConnectionText);

                        // Burası OLMADI iyice araştır
                        VTbaglanti.Open();
                        onay = true;
                        v.SP_ConnBool_Manager = true;
                    }
                    catch (Exception e2)
                    {
                        onay = false;
                        v.SP_ConnBool_Manager = false;

                        MessageBox.Show(e2.Message.ToString());

                        // yeni firma ise program kapanmasın
                        if (VTbaglanti != v.newFirm_DB.MSSQLConn)
                            Application.Exit();
                    }
                }
            }
            else
            {
                onay = true;
            }
            #endregion Closed ise

            return onay;
        }

        #endregion DB_Open

        #region Data_Read_Execute
        public void Preparing_DataSet(Form tForm, DataSet dsData, vTable vt)
        {
            // Gelen DataSet üzerinden gerekli bilgilere ulaşımaya çalışılacak;
            if (dsData != null)
            {
                string myProp = "";

                if (IsNotNull(dsData.Namespace))
                    myProp = dsData.Namespace.ToString();

                preparing_vTable(tForm, myProp, vt, dsData.Tables.Count);
            }
        }

        public v.dBaseNo getDBaseNo(string dBaseNo_Or_dBaseName)
        {
            v.dBaseNo dbNo = v.dBaseNo.None;

            string dbaseNo = dBaseNo_Or_dBaseName;

            // "master"
            if ((dBaseNo_Or_dBaseName.ToUpper() == "MASTER") || (dBaseNo_Or_dBaseName == "1"))
                dbNo = v.dBaseNo.Master;

            //  "MANAGERSERVER"
            if ((dBaseNo_Or_dBaseName.ToUpper() == v.active_DB.managerDBName.ToUpper()) || (dBaseNo_Or_dBaseName == "2"))
                dbNo = v.dBaseNo.Manager;

            //  "UstadCRM"
            if ((dBaseNo_Or_dBaseName.ToUpper() == v.active_DB.ustadCrmDBName.ToUpper()) || (dBaseNo_Or_dBaseName == "3"))
                dbNo = v.dBaseNo.UstadCrm;

            //  projenin adı
            if ((dBaseNo_Or_dBaseName.ToUpper() == v.active_DB.projectDBName.ToUpper()) || (dBaseNo_Or_dBaseName == "4") ||
                (dBaseNo_Or_dBaseName == ""))
                dbNo = v.dBaseNo.Project;

            //  local db
            if ((dBaseNo_Or_dBaseName.ToUpper() == v.active_DB.localDBName.ToUpper()) || (dBaseNo_Or_dBaseName == "5"))
                dbNo = v.dBaseNo.Local;

            //  "publishManager"
            if ((dBaseNo_Or_dBaseName.ToUpper() == v.publishManager_DB.databaseName.ToUpper()) || (dBaseNo_Or_dBaseName == "7"))
                dbNo = v.dBaseNo.publishManager;

            //  aktarılacak datanın
            if (dBaseNo_Or_dBaseName == "8")
                dbNo = v.dBaseNo.aktrilacakDb;

            return dbNo;
        }

        public void preparing_vTable(Form tForm, string myProp, vTable vt, int tableCount)
        {
            if ((IsNotNull(myProp) == false) ||
                (myProp.IndexOf("=DBaseNo:") == -1))
            {
                vt.TableName = "TABLE1";
                vt.TableCount = 255;
            }
            if (IsNotNull(myProp) && (myProp.IndexOf("=DBaseNo:") > -1))
            {
                byte dbaseNo_ = Set(MyProperties_Get(myProp, "=DBaseNo:"), "", (byte)0);

                vt.DBaseNo = getDBaseNo(dbaseNo_.ToString());    
                vt.DBaseName = Find_dBLongName(dbaseNo_.ToString());
                vt.SchemasCode = Set(MyProperties_Get(myProp, "=SchemasCode:"), "dbo", "");
                vt.TableType = Set(MyProperties_Get(myProp, "=TableType:"), "", (byte)0);
                vt.TableName = Set(MyProperties_Get(myProp, "=TableName:"), "TABLE1", "TABLE1");
                vt.TableIPCode = Set(MyProperties_Get(myProp, "=TableIPCode:"), "", "");
                vt.KeyId_FName = Set(MyProperties_Get(myProp, "=KeyFName:"), "", "");
                vt.Lock_FName = Set(MyProperties_Get(myProp, "=LockFName:"), "", "");
                vt.Cargo = Set(MyProperties_Get(myProp, "=Cargo:"), "", ""); // cargo = data, param, report

                string softwareCode = "";
                string projectCode = "";
                string TableCode = "";
                string IPCode = "";
                TableIPCode_Get(vt.TableIPCode, ref softwareCode, ref projectCode, ref TableCode, ref IPCode);
                vt.TableCode = TableCode;
                vt.IPCode = IPCode;
                vt.SoftwareCode = softwareCode;
                vt.ProjectCode = projectCode;
                vt.TableCount = (byte)tableCount;  

                if (myProp.IndexOf("Prop_Runtime:True") > 0)
                    vt.RunTime = true;
            }

            ///'TABLE_TYPE',  
            /// 1, 'Table'
            /// 2, 'View'
            /// 3, 'Stored Procedure'
            /// 4, 'Function'
            /// 5, 'Trigger'
            /// 6, 'Select'

            ///public enum dBName : byte
            ///    None,
            ///    Master,
            ///    Manager,
            ///    MainManager,
            ///    Project

            if (vt.DBaseNo == v.dBaseNo.Master)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.active_DB.masterMSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.Manager)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.active_DB.managerMSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.UstadCrm)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection =   v.active_DB.ustadCrmMSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.Project)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.Local)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.active_DB.localMSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.NewDatabase)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.publishManager)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.publishManager_DB.MSSQLConn;
            }
            if (vt.DBaseNo == v.dBaseNo.aktrilacakDb)
            {
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.msSqlConnection = v.source_DB.MSSQLConn;
            }
            if (tForm != null)
            {
                // normal formların kendi FormCode leri
                //
                if ((tForm.AccessibleName != null) && (tForm.AccessibleDescription == null))
                    vt.FormCode = tForm.AccessibleName;

                // ms_Reports formu ise onu çağıran formun FormCode si
                //
                if ((tForm.AccessibleName != null) && (tForm.AccessibleDescription != null))
                    vt.FormCode = tForm.AccessibleDescription;
            }
        }
        
        private bool IsDataTable(DataSet dsData)
        {
            // Data tablosumu kontrol et
            if (dsData.Tables.Count != 1) return false;

            if ((dsData.Tables[0].Rows.Count == 1) &&
                (dsData.Tables[0].Columns.Count == 1) &&
                (dsData.Tables[0].Columns[0].Caption == "@@VERSION")) return false;
            return true;
        }
        

        public bool Sql_ExecuteNon(string SqlText, vTable vt)
        {
            SqlCommand cmd = new SqlCommand(SqlText);

            return Sql_ExecuteNon(cmd, vt);
        }

        public Boolean Sql_ExecuteNon(SqlCommand SqlComm, vTable vt)
        {
            Boolean sonuc = false;

            if (Cursor.Current == Cursors.Default)
                Cursor.Current = Cursors.WaitCursor;

            SqlConnection msSqlConn = vt.msSqlConnection;

            // Guard: connection object must exist
            if (msSqlConn == null)
            {
                System.Diagnostics.Debug.WriteLine("Sql_ExecuteNon: vt.msSqlConnection is null. Aborting command.");
                if (Cursor.Current == Cursors.WaitCursor)
                    Cursor.Current = Cursors.Default;
                return false;
            }

            if (!Db_Open(msSqlConn))
            {
                if (Cursor.Current == Cursors.WaitCursor)
                    Cursor.Current = Cursors.Default;
                return false;
            }
                        
            try
            {
                SqlComm.Connection = msSqlConn;
                SqlComm.ExecuteNonQuery();
                sonuc = true;
            }
            catch (Exception e)
            {
                sonuc = false;
                MessageBox.Show("HATALI İŞLEM : " + v.ENTER2 + e.Message, "Sql_ExecuteNon [ " + vt.TableName + " ]");
            }

            if (sonuc == true)
            {
                try
                {
                    string Adet = "";

                    Adet = SqlComm.ExecuteNonQuery().ToString();

                    if ((Adet != "-1") && (Adet != "0"))
                        v.Kullaniciya_Mesaj_Var = Adet + " adet kayıt üzerinde işlem gerçekleşti ...";
                }
                catch
                {
                    // 
                }
            }
                        
            if (Cursor.Current == Cursors.WaitCursor)
                Cursor.Current = Cursors.Default;

            return sonuc;
        }
                
        public Boolean Sql_ExecuteNon(DataSet dsData, ref string SQL, vTable vt)
        {
            Boolean sonuc = false;

            if (Cursor.Current == Cursors.Default)
                Cursor.Current = Cursors.WaitCursor;

            if (vt.msSqlConnection == null)
                preparing_vTable(null, null, vt, 0);

            SqlConnection msSqlConn = vt.msSqlConnection;

            // Guard: connection object must exist
            if (msSqlConn == null)
            {
                System.Diagnostics.Debug.WriteLine("Sql_ExecuteNon(DataSet): vt.msSqlConnection is null after preparing_vTable. Aborting command.");
                if (Cursor.Current == Cursors.WaitCursor)
                    Cursor.Current = Cursors.Default;
                return false;
            }

            if (!Db_Open(msSqlConn))
            {
                if (Cursor.Current == Cursors.WaitCursor)
                    Cursor.Current = Cursors.Default;
                return false;
            }
            
            SqlCommand SqlComm = null;
            
            try
            {
                SQL = SQLPreparing(SQL, vt);

                if (vt.DBaseType == v.dBaseType.MSSQL)
                {
                    string Adet = "";
                    SqlComm = new SqlCommand(SQL, msSqlConn);
                    //SqlComm.ExecuteNonQuery();
                    Adet = SqlComm.ExecuteNonQuery().ToString();
                    if ((Adet != "-1") && (Adet != "0"))
                        v.Kullaniciya_Mesaj_Var = Adet + " adet kayıt üzerinde işlem gerçekleşti ...";
                }
                sonuc = true;
            }
            catch (Exception e)
            {
                sonuc = false;
                MessageBox.Show("HATALI İŞLEM : " + v.ENTER2 + e.Message, "Sql_ExecuteNon [ " + vt.TableName + " ]");
            }

            /*
            if (sonuc == true)
            {
                try
                {
                    string Adet = "";

                    Adet = SqlComm.ExecuteNonQuery().ToString();

                    if ((Adet != "-1") && (Adet != "0"))
                        v.Kullaniciya_Mesaj_Var = Adet + " adet kayıt üzerinde işlem gerçekleşti ...";
                }
                catch
                {
                    // 
                }
            }
            */

            SqlComm.Dispose();

            if (Cursor.Current == Cursors.WaitCursor)
                Cursor.Current = Cursors.Default;

            return sonuc;
        }

        public Boolean Sql_Execute(DataSet dsData, ref string SQL, vTable vt)
        {
            Boolean onay = false;

            SqlConnection msSqlConn = vt.msSqlConnection;
                        
            Db_Open(msSqlConn);

            SqlDataAdapter msSqlAdapter = null;

            #region 1. adımda DATA dolduruluyor
            try
            {
                if ((SQL.IndexOf("MS_") == -1) &&
                    (SQL.IndexOf("3S_") == -1))
                     v.SQL = v.ENTER2 + SQL + v.SQL;

                // sql execute oluyor
                //if (vt.DBaseType == v.dBaseType.MSSQL)
                msSqlAdapter = new SqlDataAdapter(SQL, msSqlConn);

                if (vt.Cargo == "data")
                {
                    if (vt.TableCount > 0)
                    {
                        bool uValue = v.con_PositionChange;
                        v.con_PositionChange = true;
                        dsData.Tables[0].Clear();
                        dsData.Tables[0].Columns.Clear();
                        v.con_PositionChange = uValue;
                    }
                    else dsData.Clear();
                }

                /// data dolduruluyor
                if (vt.DBaseType == v.dBaseType.MSSQL)
                {
                    if ((vt.TableCount == 0) && (vt.TableIPCode != ""))
                    {
                        string softwareCode = "";
                        string projectCode = "";
                        string TableCode = "";
                        string IPCode = "";
                        TableIPCode_Get(vt.TableIPCode, ref softwareCode, ref projectCode, ref TableCode, ref IPCode);

                        //msSqlAdapter.Fill(dsData, vt.TableName);
                        msSqlAdapter.Fill(dsData, IPCode);

                        //dsData.Tables[0].TableName = name;
                        //dsData.Tables[0].TableName = name;
                    }
                    else if ((vt.TableCount == 2) && (vt.IPCode != ""))
                    {
                        msSqlAdapter.Fill(dsData, vt.IPCode);
                    }
                    else
                    {
                        string name = vt.TableName;//.Replace(".", "_");
                        msSqlAdapter.Fill(dsData, name);
                    } 
                        

                }

                if ((vt.Cargo == "data") &&
                    (SQL.IndexOf("Select @@VERSION") == -1))
                {
                    tSqlSecond_Set(ref dsData, SQL);
                }

                onay = true;

            }
            catch (Exception e)
            {
                /// uzak bağlantıda ilk insert denemesinde hata veriyor
                /// o hata oluştursa aynı sql bir daha çalışınca
                /// işlem bu sefer gerçekleşiyor
                /// bu geçici çözümdür
                ///

                onay = false;
                Cursor.Current = Cursors.Default;

                v.SQL = v.ENTER2 +
                    "   HATALI SQL :  [ " + e.Message.ToString() + v.ENTER2 +
                    vt.TableName + " / " + vt.TableIPCode + " / " + vt.functionName +
                    " ] { " + v.ENTER2 + SQL + v.ENTER2 + " } " + v.SQL;

                //MessageBox.Show("HATALI SQL : " + v.ENTER2 + e.Message.ToString() +
                //    v.ENTER2 + vt.TableName + v.ENTER2 + SQL, "Data_Read_Execute");
                AlertMessage("Konu", vt.TableName + " / " + vt.TableIPCode);
            }

            #endregion 1.adım

            if (msSqlAdapter != null) msSqlAdapter.Dispose();

            return onay;
        }

        public Boolean Data_Read_Execute(Form tForm,
                                         DataSet dsData,
                                         ref string SQL, string TableName,
                                         Control cntrl)
        {
            Boolean onay = false;

            // -99 var ise SQL çalışmasın 
            int Eksi99 = SQL.IndexOf("-99");

            if (Cursor.Current == Cursors.Default)
                Cursor.Current = Cursors.WaitCursor;

            // Gerekli olan verileri topla
            vTable vt = new vTable();
            Preparing_DataSet(tForm, dsData, vt);

            if ((SQL != "") &&
                (Eksi99 == -1))
            {
                SQL = SQLPreparing(SQL, vt);

                // 1. adım
                onay = Sql_Execute(dsData, ref SQL, vt);

                DataNavigator dN = Find_DataNavigator(tForm, vt.TableIPCode);
                if (dN != null)
                    dN.Tag = dN.Position;

                // buraya yeni kondu
                // table refreshde çalışmadığı tespit edildi
                // 
                tSqlSecond_Set(ref dsData, SQL);
            }

            #region NotExecute
            if ((SQL != "") &&
                (Eksi99 > -1))
            {
                SQL = SQLPreparing(SQL, vt);

                // yukaı taşınıca kapatıldı
                //
                //tSqlSecond_Set(ref dsData, SQL);
                
                if ((vt.TableName.IndexOf("SNL_") > -1) ||
                    (vt.TableName.IndexOf("3S_") > -1))//  || (vt.TableType == 3) // stored procedure 
                    SQL = "Select @@VERSION ";
                    
                // 1. adım
                onay = Sql_Execute(dsData, ref SQL, vt);
                if (IsNotNull(dsData))
                    SQL = dsData.Tables[0].Rows.Count.ToString() + " adet kayıt ... [NotExecute]";

            }
            #endregion NotExecute

            #region 2. adımda Tablonun Fields bilgileri geliyor
            //Task task1 = new Task(() =>
            //{
            preparingTableAndFields(tForm, dsData);
            //});
            //task1.Start();

            #endregion 2. adım

            if (Cursor.Current == Cursors.WaitCursor)
                Cursor.Current = Cursors.Default;
            
            if (cntrl != null)
            {
                if (cntrl.GetType().ToString().IndexOf("DevExpress.XtraGrid.GridControl") > -1)
                    GridGroupRefresh(cntrl);
            }

            return onay;
        }

        public void preparingTableAndFields(Form tForm, DataSet dsData)
        {
            vTable vt = new vTable();
            Preparing_DataSet(tForm, dsData, vt);

            /// tablo ilk defa okunuyorsa ve
            /// data tablosuyla tablonun fields listesi hazırlanıyor 
            if ((vt.TableCount == 1) &&
                (vt.Cargo == "data"))
            {
                if (IsDataTable(dsData))
                {
                    if ((vt.TableType != 1) || // table
                        (vt.TableType != 3) || // storedprocedure
                        (vt.TableType != 6)    // select
                       )
                    {
                        preparing_MsTableFields(vt);
                        preparing_TableIPCodeTableList(vt.TableIPCode);
                        preparing_TableIPCodeFieldsList(vt.TableIPCode);
                        if (IsNotNull(vt.TableIPCode))
                        {
                            if (v.active_DB.mainManagerDbUses || vt.DBaseNo == v.dBaseNo.UstadCrm)
                            {
                                DataTable dt = v.ds_TableIPCodeFields.Tables[vt.TableIPCode];
                                if (dt != null)
                                {
                                    dsData.Tables.Add(dt.Copy());
                                    dt.Dispose();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion Data_Read_Execute

        #region Read : msTables, msTableIPCodeTables, msTableIPCodeFields, msTableIPGroups
        public void preparing_MsTableFields(vTable vt)
        {
            //string fields = "_FIELDS";
            string sqlA = string.Empty;

            //var matchingTable = v.tableList.Where(stringToCheck => stringToCheck.Contains(vt.TableName));
            var matchingTable = v.tableList.Where(stringToCheck => stringToCheck.Equals(vt.TableName));
            if (matchingTable.Any())
            {
                // bu table için fieldlist1 daha önce yüklenmiş demektir
                // bir daha okumaya gerek yok
                return;
            }
            else
            {
                v.tableList.Add(vt.TableName);

                sqlA = msTableFieldsList_SQL(vt.TableName);

                SqlDataAdapter msSqlAdapter = null;
                if (sqlA != string.Empty)
                {
                    msSqlAdapter = new SqlDataAdapter(sqlA, vt.msSqlConnection);
                    //msSqlAdapter.Fill(dsData, vt.TableName + fields);
                    msSqlAdapter.Fill(v.ds_MsTableFields, vt.TableName);
                }
                msSqlAdapter.Dispose();
            }

        }
        public void preparing_TableIPCodeTableList(string tableIPCode)
        {
            if (IsNotNull(tableIPCode) == false) return;

            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_TableIPCodeTable);
                readTableIPCodeTableList_(tableIPCode);
            }
            else
            {
                var matchingTableIPCode = v.tableIPCodeTableList.Where(stringToCheck => stringToCheck.Contains(tableIPCode));
                if (matchingTableIPCode.Any())
                {
                    // bu tableIPCode için table bilgileri daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.tableIPCodeTableList.Add(tableIPCode);
                    readTableIPCodeTableList_(tableIPCode);
                }
            }
        }
        private void readTableIPCodeTableList_(string tableIPCode)
        {
            if (IsNotNull(tableIPCode) == false) return;
            string sqlB = msTableIPCodeTableList_SQL(tableIPCode);
            SqlDataAdapter msSqlAdapter2 = null;
            if (sqlB != string.Empty)
            {
                msSqlAdapter2 = new SqlDataAdapter(sqlB, v.active_DB.managerMSSQLConn);
                //_FIELDS2
                //msSqlAdapter.Fill(dsData, vt.TableName + fields + "2");
                msSqlAdapter2.Fill(v.ds_TableIPCodeTable, tableIPCode);// vt.TableName + fields + "2");
            }
            msSqlAdapter2.Dispose();
        }
        public void preparing_TableIPCodeFieldsList(string tableIPCode)
        {
            if (IsNotNull(tableIPCode) == false) return;
            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_TableIPCodeFields);
                readTableIPCodeFieldsList_(tableIPCode);
            }
            else
            {
                var matchingTableIPCode = v.tableIPCodeFieldsList.Where(stringToCheck => stringToCheck.Contains(tableIPCode));
                if (matchingTableIPCode.Any())
                {
                    // bu tableIPCode için field list daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.tableIPCodeFieldsList.Add(tableIPCode);
                    readTableIPCodeFieldsList_(tableIPCode);
                }
            }
        }
        private void readTableIPCodeFieldsList_(string tableIPCode)
        {
            if (IsNotNull(tableIPCode) == false) return;
            string sqlC = msTableIPCodeFieldsList_SQL(tableIPCode);
            SqlDataAdapter msSqlAdapter3 = null;
            if (sqlC != string.Empty)
            {
                msSqlAdapter3 = new SqlDataAdapter(sqlC, v.active_DB.managerMSSQLConn);
                //_FIELDS2
                //msSqlAdapter.Fill(dsData, vt.TableName + fields + "2");
                msSqlAdapter3.Fill(v.ds_TableIPCodeFields, tableIPCode);// vt.TableName + fields + "2");
            }
            msSqlAdapter3.Dispose();
        }
        public void preparing_TableIPCodeGroupsList(string tableIPCode)
        {
            if (IsNotNull(tableIPCode) == false) return;

            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_TableIPCodeGroups);
                readTableIPCodeGroupsList_(tableIPCode);
            }
            else
            {
                var matchingTableIPCode = v.tableIPCodeGroupsList.Where(stringToCheck => stringToCheck.Contains(tableIPCode));
                if (matchingTableIPCode.Any())
                {
                    // bu tableIPCode için groups list daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.tableIPCodeGroupsList.Add(tableIPCode);
                    readTableIPCodeGroupsList_(tableIPCode);
                }
            }
        }
        private void readTableIPCodeGroupsList_(string tableIPCode)
        {
            if (IsNotNull(tableIPCode) == false) return;
            string sqlC = msTableIPCodeGroupsList_SQL(tableIPCode);
            SqlDataAdapter msSqlAdapter4 = null;
            if (sqlC != string.Empty)
            {
                msSqlAdapter4 = new SqlDataAdapter(sqlC, v.active_DB.managerMSSQLConn);
                msSqlAdapter4.Fill(v.ds_TableIPCodeGroups, tableIPCode + "_GROUPS");
            }
            msSqlAdapter4.Dispose();
        }
        public void preparing_LayoutItemsList(string masterCode)
        {
            if (IsNotNull(masterCode) == false) return;
            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_MsLayoutItems);
                readLayoutItemsList_(masterCode);
            }
            else
            {
                var matchingMasterCode = v.msLayoutItemsList.Where(stringToCheck => stringToCheck.Contains(masterCode));
                if (matchingMasterCode.Any())
                {
                    // bu Layout için items list daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.msLayoutItemsList.Add(masterCode);
                    readLayoutItemsList_(masterCode);
                }
            }
        }
        private void readLayoutItemsList_(string masterCode)
        {
            if (IsNotNull(masterCode) == false) return;
            string sqlL = msLayoutItemsList_SQL(masterCode);
            // TODO(@Janberk): Move layout metadata to API/local cache so login can render without opening DB.
            using (var msSqlAdapter5 = new SqlDataAdapter(sqlL, v.active_DB.managerMSSQLConn))
            {
                msSqlAdapter5.Fill(v.ds_MsLayoutItems, masterCode);
            }
        }
        public void preparing_MenuItemsList(string masterCode)
        {
            if (IsNotNull(masterCode) == false) return;

            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_MsMenuItems);
                readMenuItemsList_(masterCode);
            }
            else
            {
                var matchingMasterCode = v.msMenuItemsList.Where(stringToCheck => stringToCheck.Contains(masterCode));
                if (matchingMasterCode.Any())
                {
                    // bu Menu için items list daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.msMenuItemsList.Add(masterCode);
                    readMenuItemsList_(masterCode);
                }
            }
        }
        private void readMenuItemsList_(string masterCode)
        {
            if (IsNotNull(masterCode) == false) return;

            string sqlM = msMenuItemsList_SQL(masterCode);

            SqlDataAdapter msSqlAdapter6 = null;
            if (sqlM != string.Empty)
            {
                msSqlAdapter6 = new SqlDataAdapter(sqlM, v.active_DB.managerMSSQLConn);
                msSqlAdapter6.Fill(v.ds_MsMenuItems, masterCode);
            }
            msSqlAdapter6.Dispose();
        }
        public void preparing_DataCopyList(string DC_Code)
        {
            if (IsNotNull(DC_Code) == false) return;

            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_DataCopy);
                readDataCopyList_(DC_Code);
            }
            else
            {
                var matchingDCCode = v.dataCopyList.Where(stringToCheck => stringToCheck.Contains(DC_Code));
                if (matchingDCCode.Any())
                {
                    // bu DataCopy için tanımlar daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.dataCopyList.Add(DC_Code);
                    readDataCopyList_(DC_Code);
                }
            }
        }
        private void readDataCopyList_(string DC_Code)
        {
            if (IsNotNull(DC_Code) == false) return;

            string sqlDC = dataCopyList_SQL(DC_Code);

            SqlDataAdapter msSqlAdapter7 = null;
            if (sqlDC != string.Empty)
            {
                msSqlAdapter7 = new SqlDataAdapter(sqlDC, v.active_DB.managerMSSQLConn);
                msSqlAdapter7.Fill(v.ds_DataCopy, DC_Code);
            }
            msSqlAdapter7.Dispose();
        }
        public void preparing_DataCopyLinesList(string DC_Code)
        {
            if (IsNotNull(DC_Code) == false) return;

            if (v.active_DB.mainManagerDbUses)
            {
                TableRemove(v.ds_DataCopyLines);
                readDataCopyLinesList_(DC_Code);
            }
            else
            {
                var matchingDCCode = v.dataCopyLinesList.Where(stringToCheck => stringToCheck.Contains(DC_Code));
                if (matchingDCCode.Any())
                {
                    // bu DataCopyLines için list daha önce yüklenmiş demektir
                    // bir daha okumaya gerek yok
                    return;
                }
                else
                {
                    v.dataCopyLinesList.Add(DC_Code);
                    readDataCopyLinesList_(DC_Code);
                }
            }
        }
        private void readDataCopyLinesList_(string DC_Code)
        {
            if (IsNotNull(DC_Code) == false) return;

            string sqlDCL = dataCopyLinesList_SQL(DC_Code);

            SqlDataAdapter msSqlAdapter8 = null;
            if (sqlDCL != string.Empty)
            {
                msSqlAdapter8 = new SqlDataAdapter(sqlDCL, v.active_DB.managerMSSQLConn);
                msSqlAdapter8.Fill(v.ds_DataCopyLines, DC_Code);
            }
            msSqlAdapter8.Dispose();
        }

        //--- SQLs
        private string msTableFieldsList_SQL(string tableName)
        {
            return @" 
             Select  
               a.column_id, a.name 
             , convert(smallInt, a.system_type_id) as system_type_id 
             , convert(smallInt, a.user_type_id)   as user_type_id 
             , a.max_length, a.precision, a.scale, a.is_nullable, a.is_identity  
             from sys.columns a With (nolock) 
               left outer join sys.tables b With (nolock) on (a.object_id = b.object_id ) 
             where b.name = '" + tableName + @"' 
             order by a.column_id ";

        }
        public string msTableIPCodeTableList_SQL(string tableIPCode)
        {
            string softCode = "";
            string projectCode = "";
            string TableCode = string.Empty;
            string IPCode = string.Empty;

            TableIPCode_Get(tableIPCode, ref softCode, ref projectCode, ref TableCode, ref IPCode);

            string tSql =
              @" Select a.*  
               , b.SCHEMAS_CODE        LKP_SCHEMAS_CODE   
               , b.DBASE_TYPE          LKP_DBASE_TYPE 
               , b.MODUL_CODE          LKP_MODUL_CODE 
               , b.PARENT_TABLE_CODE   LKP_PARENT_TABLE_CODE 
               , b.TABLE_CODE          LKP_TABLE_CODE 
               , b.TABLE_NAME          LKP_TABLE_NAME 
               , b.KEY_FNAME           LKP_KEY_FNAME  
               , b.TABLE_TYPE          LKP_TABLE_TYPE 

               , b.MASTER_TABLEIPCODE  LKP_MASTER_TABLEIPCODE 
               , b.MASTER_TABLE_NAME   LKP_MASTER_TABLE_NAME 
               , b.MASTER_KEY_FNAME    LKP_MASTER_KEY_FNAME 
              
               , b.FOREING_FNAME       LKP_FOREING_FNAME 
               , b.PARENT_FNAME        LKP_PARENT_FNAME 
               , b.TB_CAPTION          LKP_TB_CAPTION 
               , b.TB_SELECT_SQL       LKP_TB_SELECT_SQL 
               , b.TB_WHERE_SQL        LKP_TB_WHERE_SQL 
               , b.TB_ORDER_BY         LKP_TB_ORDER_BY 
               , b.LOCK_FIELD_NAME     LKP_LOCK_FIELD_NAME
               , b.PROP_SUBVIEW        LKP_PROP_SUBVIEW 
               , b.PROP_JOINTABLE      LKP_PROP_JOINTABLE 
                             
               from MS_TABLES_IP a With (nolock)
                 left outer join MS_TABLES b With (nolock) on ( 
                        a.TABLE_CODE = b.TABLE_CODE 
                    and a.SOFTWARE_CODE = b.SOFTWARE_CODE
                    and a.PROJECT_CODE = b.PROJECT_CODE
                    ) 
               where 0 = 0 ";

            if ((TableCode != "") && (TableCode != "null"))
                tSql += " and a.TABLE_CODE = '" + TableCode + "' ";

            if ((IPCode != "") && (IPCode != "null"))
                tSql += " and a.IP_CODE = '" + IPCode + "' ";

            if ((softCode != "") && (softCode != "null"))
                tSql += " and a.SOFTWARE_CODE = '" + softCode + "' ";

            if ((projectCode != "") && (projectCode != "null"))
                tSql += " and a.PROJECT_CODE = '" + projectCode + "' ";

            return tSql;
        }
        public string msTableIPCodeFieldsList_SQL(string tableIPCode)
        {
            //public string SQL_MS_FIELDS_IP_LIST(string Table_IP_Code)
            string softCode = "";
            string projectCode = "";
            string tableCode = string.Empty;
            string IPCode = string.Empty;

            TableIPCode_Get(tableIPCode, ref softCode, ref projectCode, ref tableCode, ref IPCode);

            string msfields_list = MS_FIELDS_LIST("b");

            string tSql =
              " Select a.* " + v.ENTER
            + " , c.TABLE_NAME as LKP_TABLE_NAME " + v.ENTER
            + " , c.DBASE_TYPE as LKP_DBASE_TYPE " + v.ENTER

            + msfields_list
            + " from MS_FIELDS_IP a With (nolock) " + v.ENTER
            + "   left outer join MS_FIELDS b With (nolock) on ( a.TABLE_CODE = b.TABLE_CODE and a.FIELD_NO = b.FIELD_NO ) " + v.ENTER
            + "   left outer join MS_TABLES c With (nolock) on ( a.TABLE_CODE = c.TABLE_CODE ) " + v.ENTER
            + " where a.TABLE_CODE = '" + tableCode + "' " + v.ENTER
            + " and   a.IP_CODE = '" + IPCode + "' " + v.ENTER;

            if ((softCode != "") && (softCode != "null"))
                tSql += " and   a.SOFTWARE_CODE = '" + softCode + "' " + v.ENTER
                      + " and   a.SOFTWARE_CODE = b.SOFTWARE_CODE " + v.ENTER
                      + " and   a.SOFTWARE_CODE = c.SOFTWARE_CODE " + v.ENTER;

            if ((projectCode != "") && (projectCode != "null"))
                tSql += " and   a.PROJECT_CODE = '" + projectCode + "' " + v.ENTER
                      + " and   a.PROJECT_CODE = b.PROJECT_CODE " + v.ENTER
                      + " and   a.PROJECT_CODE = c.PROJECT_CODE " + v.ENTER;

            tSql +=
              " order by "
            + " isnull(a.GROUP_NO,0), isnull(a.GROUP_LINE_NO,0), "
            + " isnull(b.GROUP_NO,0), isnull(b.GROUP_LINE_NO,0), "
            + " isnull(a.FIELD_NO,0) ";

            return tSql;
            #region
            /*
                        sqlB =
                        @" select  
                           d.FIELD_NO
                         , d.FIELD_NAME
                         , d.FFOREING 
                         , d.FTRIGGER 
                         , d.PROP_EXPRESSION 
                         , e.VALIDATION_INSERT
                         , e.XML_FIELD_NAME
                         , e.CMP_DISPLAY_FORMAT
                         , e.CMP_EDIT_FORMAT
                         , e.CMP_VISIBLE
                         , e.DEFAULT_TYPE
                         from dbo.MS_TABLES as c 
                           left outer join dbo.MS_FIELDS as d on ( 
                               c.TABLE_CODE = d.TABLE_CODE 
                           and c.SOFTWARE_CODE = d.SOFTWARE_CODE
                           and c.PROJECT_CODE = d.PROJECT_CODE
                           )
                           left outer join dbo.MS_FIELDS_IP as e on (
                               d.TABLE_CODE = e.TABLE_CODE 
                           and d.SOFTWARE_CODE = e.SOFTWARE_CODE
                           and d.PROJECT_CODE = e.PROJECT_CODE
                           and d.FIELD_NO = e.FIELD_NO
                           and e.IP_CODE = '" + vt.IPCode + @"' )  
                         where c.TABLE_NAME = '" + vt.TableName + @"' 
                         and   c.SOFTWARE_CODE = '" + vt.SoftwareCode + @"'
                         and   c.PROJECT_CODE = '" + vt.ProjectCode + @"'
                         and   c.TABLE_CODE = '" + vt.TableCode + @"'
                         order by d.FIELD_NO ";

                        if ((vt.TableName.IndexOf("_KIST") > -1) ||
                            (vt.TableName.IndexOf("_FULL") > -1))
                            MessageBox.Show("Preparing_Fields_SQL : Table_Name.IndexOf(_KIST) / (_FULL) : konusu vardı.");
            */
            #endregion

        }
        public string MS_FIELDS_LIST(string Alias)
        {

            string s = v.ENTER
           + " , " + Alias + ".TABLE_CODE               LKP_TABLE_CODE " + v.ENTER
           + " , " + Alias + ".FIELD_NO                 LKP_FIELD_NO " + v.ENTER
           + " , " + Alias + ".FIELD_NAME               LKP_FIELD_NAME " + v.ENTER
           + " , " + Alias + ".FIELD_TYPE               LKP_FIELD_TYPE " + v.ENTER
           + " , " + Alias + ".FIELD_LENGTH             LKP_FIELD_LENGTH " + v.ENTER
           + " , " + Alias + ".FAUTOINC                 LKP_FAUTOINC " + v.ENTER
           + " , " + Alias + ".FNOTNULL                 LKP_FNOTNULL " + v.ENTER
           + " , " + Alias + ".FREADONLY                LKP_FREADONLY " + v.ENTER
           + " , " + Alias + ".FENABLED                 LKP_FENABLED " + v.ENTER
           + " , " + Alias + ".FVISIBLE                 LKP_FVISIBLE " + v.ENTER
           + " , " + Alias + ".FINDEX                   LKP_FINDEX " + v.ENTER
           + " , " + Alias + ".FPICTURE                 LKP_FPICTURE " + v.ENTER
           + " , " + Alias + ".FTRIGGER                 LKP_FTRIGGER " + v.ENTER
           + " , " + Alias + ".FFOREING                 LKP_FFOREING " + v.ENTER
           + " , " + Alias + ".FMEMORY_FIELD            LKP_FMEMORY_FIELD " + v.ENTER
           + " , " + Alias + ".FLOOKUP_FIELD            LKP_FLOOKUP_FIELD " + v.ENTER

           + " , " + Alias + ".FCAPTION                 LKP_FCAPTION " + v.ENTER
           //+ " , " + Alias + ".FHINT                    LKP_FHINT " + v.ENTER

           + " , " + Alias + ".DEFAULT_TYPE             LKP_DEFAULT_TYPE " + v.ENTER
           //+ " , " + Alias + ".DEFAULT_TYPE2            LKP_DEFAULT_TYPE2 " + v.ENTER
           + " , " + Alias + ".DEFAULT_NUMERIC          LKP_DEFAULT_NUMERIC " + v.ENTER
           + " , " + Alias + ".DEFAULT_TEXT             LKP_DEFAULT_TEXT " + v.ENTER
           + " , " + Alias + ".DEFAULT_INT              LKP_DEFAULT_INT " + v.ENTER
           + " , " + Alias + ".DEFAULT_SP               LKP_DEFAULT_SP " + v.ENTER
           + " , " + Alias + ".DEFAULT_SETUP            LKP_DEFAULT_SETUP " + v.ENTER

           + " , " + Alias + ".CMP_COLUMN_TYPE          LKP_CMP_COLUMN_TYPE " + v.ENTER
           + " , " + Alias + ".CMP_WIDTH                LKP_CMP_WIDTH " + v.ENTER
           + " , " + Alias + ".CMP_SORT_TYPE            LKP_CMP_SORT_TYPE " + v.ENTER
           + " , " + Alias + ".CMP_SUMMARY_TYPE         LKP_CMP_SUMMARY_TYPE " + v.ENTER
           + " , " + Alias + ".CMP_FORMAT_TYPE          LKP_CMP_FORMAT_TYPE " + v.ENTER
           + " , " + Alias + ".CMP_DISPLAY_FORMAT       LKP_CMP_DISPLAY_FORMAT " + v.ENTER
           + " , " + Alias + ".CMP_EDIT_FORMAT          LKP_CMP_EDIT_FORMAT " + v.ENTER
           + " , " + Alias + ".LIST_TYPES_NAME          LKP_LIST_TYPES_NAME " + v.ENTER
           + " , " + Alias + ".VALIDATION_OPERATOR      LKP_VALIDATION_OPERATOR " + v.ENTER
           + " , " + Alias + ".VALIDATION_VALUE1        LKP_VALIDATION_VALUE1 " + v.ENTER
           + " , " + Alias + ".VALIDATION_VALUE2        LKP_VALIDATION_VALUE2 " + v.ENTER
           + " , " + Alias + ".VALIDATION_ERRORTEXT     LKP_VALIDATION_ERRORTEXT " + v.ENTER
           + " , " + Alias + ".VALIDATION_ERRORTYPE     LKP_VALIDATION_ERRORTYPE " + v.ENTER

           //+ " , " + Alias + ".KRT_LINE_NO              LKP_KRT_LINE_NO " + v.ENTER
           //+ " , " + Alias + ".KRT_CAPTION              LKP_KRT_CAPTION " + v.ENTER // Kriter FieldName olara kullnaılıyor
           //+ " , " + Alias + ".KRT_OPERAND_TYPE         LKP_KRT_OPERAND_TYPE " + v.ENTER
           //+ " , " + Alias + ".KRT_LIKE                 LKP_KRT_LIKE " + v.ENTER
           //+ " , " + Alias + ".KRT_DEFAULT1             LKP_KRT_DEFAULT1 " + v.ENTER
           //+ " , " + Alias + ".KRT_DEFAULT2             LKP_KRT_DEFAULT2 " + v.ENTER
           //+ " , " + Alias + ".KRT_ALIAS                LKP_KRT_ALIAS " + v.ENTER
           //+ " , " + Alias + ".KRT_TABLE_ALIAS          LKP_KRT_TABLE_ALIAS " + v.ENTER

           //+ " , " + Alias + ".MASTER_TABLEIPCODE       LKP_MASTER_TABLEIPCODE " + v.ENTER
           //+ " , " + Alias + ".SEARCH_TABLEIPCODE       LKP_MASTER_TABLE_NAME " + v.ENTER
           //+ " , " + Alias + ".MASTER_KEY_FNAME         LKP_MASTER_KEY_FNAME " + v.ENTER
           //+ " , " + Alias + ".MASTER_CHECK_FNAME       LKP_MASTER_CHECK_FNAME " + v.ENTER
           //+ " , " + Alias + ".MASTER_CHECK_VALUE       LKP_MASTER_CHECK_VALUE " + v.ENTER

           + " , " + Alias + ".GROUP_NO                 LKP_GROUP_NO " + v.ENTER
           + " , " + Alias + ".GROUP_LINE_NO            LKP_GROUP_LINE_NO " + v.ENTER

           + " , " + Alias + ".EXPRESSION_TYPE          LKP_EXPRESSION_TYPE " + v.ENTER
           + " , " + Alias + ".PROP_EXPRESSION          LKP_PROP_EXPRESSION " + v.ENTER // LKP_EXPRESSION

           // searchLookUpTableFill sırasında gerekiyor
           + " , " + Alias + ".FJOIN_TABLE_NAME         LKP_FJOIN_TABLE_NAME " + v.ENTER 
           + " , " + Alias + ".FJOIN_TABLE_ALIAS        LKP_FJOIN_TABLE_ALIAS " + v.ENTER
           + " , " + Alias + ".FJOIN_KEY_FNAME          LKP_FJOIN_KEY_FNAME " + v.ENTER
           + " , " + Alias + ".FJOIN_CAPTION_FNAME      LKP_FJOIN_CAPTION_FNAME " + v.ENTER
           + " , " + Alias + ".FJOIN_WHERE              LKP_FJOIN_WHERE " + v.ENTER
           ;

            return s;
        }
        public string msTableIPCodeGroupsList_SQL(string tableIPCode)
        {
            //public string SQL_MS_GROUPS(string Table_IP_Code)
            string softCode = "";
            string projectCode = "";
            string tableCode = string.Empty;
            string IPCode = string.Empty;

            TableIPCode_Get(tableIPCode, ref softCode, ref projectCode, ref tableCode, ref IPCode);

            string tSql =
               @" Select x.* from ( 
               select
                 a.MAIN_TABLE_NAME
               , a.TABLE_CODE
               , a.GROUP_TYPES
               , a.FCAPTION
               , a.FGROUPNO
               , a.FVISIBLE
               , a.FIXED
               , a.CMP_WIDTH
               , a.MOVE_ITEM_TYPE
               , a.MOVE_ITEM_NAME
               , a.MOVE_TYPE
               , a.MOVE_LOCATION
               , a.MOVE_LAYOUT_TYPE
               , a.PROP_VIEWS
               , 1 TABLE_NO

               from MS_GROUPS a With (nolock)
               where a.MAIN_TABLE_NAME = 'MS_FIELDS_IP'
               and a.TABLE_CODE = '" + tableCode + @"'
               and a.IP_CODE = '" + IPCode + @"'
               and a.SOFTWARE_CODE = '" + softCode + @"'
               and a.PROJECT_CODE = '" + projectCode + @"'

               union all

               select
                 a.MAIN_TABLE_NAME
               , a.TABLE_CODE
               , a.GROUP_TYPES
               , a.FCAPTION
               , a.FGROUPNO
               , a.FVISIBLE
               , a.FIXED
               , a.CMP_WIDTH
               , a.MOVE_ITEM_TYPE
               , a.MOVE_ITEM_NAME
               , a.MOVE_TYPE
               , a.MOVE_LOCATION
               , a.MOVE_LAYOUT_TYPE
               , a.PROP_VIEWS
               , 2 TABLE_NO

               from MS_GROUPS a With (nolock)
               where a.MAIN_TABLE_NAME = 'MS_FIELDS'
               and a.TABLE_CODE = '" + tableCode + @"'
               and a.SOFTWARE_CODE = '" + softCode + @"'
               and a.PROJECT_CODE = '" + projectCode + @"'
               ) x

              order by x.TABLE_NO, x.FGROUPNO ";

            return tSql;
        }
        public string msLayoutItemsList_SQL(string masterCode)
        {
            // public string SQL_MS_LAYOUT_LIST(string MasterCode, byte MasterItemType)

            //if (MasterItemType > 0)
            //    myAnd = " and a.MASTER_ITEM_TYPE = " + MasterItemType.ToString() + " ";

            string tSql =
            @" select a.* from MS_LAYOUT a With (nolock)
               where a.MASTER_CODE = '" + masterCode + @"' 
               order by a.MASTER_CODE, isnull(a.GROUP_LINE_NO,0), isnull(a.LAYOUT_CODE,0) ";
            return tSql;
        }
        public string msMenuItemsList_SQL(string masterCode)
        {
            // public string SQL_MS_ITEMS_LIST(string MasterCode, byte MasterItemType)
            // and a.MASTER_ITEM_TYPE = " + MasterItemType.ToString() +  

            string tSql =
            @"  select a.* 
                , g16.GLYPH LKP_GLYPH16 
                , g32.GLYPH LKP_GLYPH32 
                from MS_ITEMS a With (nolock)
                  left outer join MS_GLYPH g16 With (nolock) on (a.GYLPH_16 = g16.GLYPH_NAME) 
                  left outer join MS_GLYPH g32 With (nolock) on (a.GYLPH_32 = g32.GLYPH_NAME) 
               where a.MASTER_CODE = '" + masterCode + @"' 
               and a.MASTER_ITEM_TYPE = 3
               order by a.MASTER_CODE, a.ITEM_CODE ";

            return tSql;
        }
        public string dataCopyList_SQL(string DC_Code)
        {
            return
             @" Select a.* from [dbo].[MS_DC] as a With (nolock) Where  a.DC_CODE = '" + DC_Code + @"' ";
        }
        public string dataCopyLinesList_SQL(string DC_Code)
        {
            return
              @" Select a.* from [dbo].[MS_DC_LINE] as a With (nolock) Where  a.DC_CODE = '" + DC_Code + @"' Order by a.LINE_NO ";
        }
        
        //--- SQLs end
        #endregion Read : msTables, msTableIPCodeTables, msTableIPCodeFields, msTableIPGroups

        #region SQL_Read_Execute
        public Boolean SQL_Read_Execute(v.dBaseNo dBNo, DataSet dsData,
               ref string SQL, string tableName, string function_name)
        {
            return SQL_Read_Execute(null, dBNo, dsData, ref SQL, tableName, function_name);
        }

        public Boolean SQL_Read_Execute(Form tForm, v.dBaseNo dBNo, DataSet dsData, ref string SQL, string tableName, string function_name)
        {
            Boolean onay = false;
            string TableIPCode = string.Empty;

            if (Cursor.Current == Cursors.Default) 
                Cursor.Current = Cursors.WaitCursor;

            // Gerekli olan verileri topla
            vTable vt = new vTable();
            vt.Clear();
            vt.functionName = function_name;
            vt.DBaseNo = dBNo;

            Preparing_DataSet(tForm, dsData, vt);

            if (SQL != "")
            {

                if ((vt.TableName == "TABLE1") &&
                    (tableName != ""))
                    vt.TableName = tableName;

                if (vt.DBaseNo != v.dBaseNo.publishManager)
                    SQL = SQLPreparing(SQL, vt);

                if ((tableName != "GROUPS") &&
                    (SQL.IndexOf("[Lkp]") == -1))
                {
                    // TableRemove daha önce çalışmıyormuş
                    // şimdi çalışmaya başladı
                    // bu seferde silmemesi gerekenleride siliyor
                    // ne zaman silecek doğru tespit et
                    // 2023.12.24
                    //TableRemove(dsData);
                }

                // 1. adım
                onay = Sql_Execute(dsData, ref SQL, vt);

                tSqlSecond_Set(ref dsData, SQL);

                if (IsNotNull(vt.TableIPCode))
                    dsData.DataSetName = vt.TableIPCode;
            }

            if (Cursor.Current == Cursors.WaitCursor)
                Cursor.Current = Cursors.Default;

            return onay;
            /*
            SqlConnection conn = new SqlConnection(sqlConnectionString); 
            Server server = new Server(new ServerConnection(conn)); 
            server.ConnectionContext.ExecuteNonQuery(script);
            */
        }
        #endregion SQL_Read_Execute
                
        #region TableRemove
        public Boolean TableRemove(DataSet ds)
        {
            Boolean sonuc = true;

            if (ds == null) return sonuc;
            if (ds.Tables.Count == 0) return sonuc;

            try
            {
                for (int i = ds.Tables.Count - 1; i >= 0; i--)
                {
                    //ds.Tables[i].Clear();
                    //ds.Tables.RemoveAt(i);
                    ds.Tables[i].Rows.Clear();
                    ds.Tables.Remove(ds.Tables[i]);
                }
            }
            catch (Exception e)
            {
                sonuc = false;
                MessageBox.Show("HATALI : " + v.ENTER2 + e.Message, "TableRemove");
            }

            return sonuc;
        }
        #endregion TableRemove
        
        #region TableFieldsListRefresh
        public void TableFieldsListRefresh(Form tForm, DataSet dsData)
        {
            if (dsData == null) return;

            vTable vt = new vTable();
            Preparing_DataSet(tForm, dsData, vt);

            var matchingTable = v.tableList.Where(stringToCheck => stringToCheck.Contains(vt.TableName));
            if (matchingTable.Any())
            {
                v.tableList.Remove(vt.TableName);
                TableRemoveOnDataSet(v.ds_MsTableFields,vt.TableName);
            }
            /* burayı silme gerek olunca kullanırsın
             * 
            var matchingTableIPCode = v.tableIPCodeTableList.Where(stringToCheck => stringToCheck.Contains(vt.TableIPCode));
            if (matchingTableIPCode.Any())
            {
                v.tableIPCodeTableList.Remove(vt.TableIPCode);
                TableRemoveOnDataSet(v.ds_TableIPCodeTable, vt.TableIPCode);
            }
            var matchingTableIPCode2 = v.tableIPCodeFieldsList.Where(stringToCheck => stringToCheck.Contains(vt.TableIPCode));
            if (matchingTableIPCode2.Any())
            {
                v.tableIPCodeFieldsList.Remove(vt.TableIPCode);
                TableRemoveOnDataSet(v.ds_TableIPCodeFields, vt.TableIPCode);
            }
            */
            preparingTableAndFields(tForm, dsData);
        }

        public void TableRemoveOnDataSet(DataSet ds, string name)
        {
            if (ds != null)
            {
                ds.Tables[name].Rows.Clear();
                ds.Tables.Remove(ds.Tables[name]);
            }
        }
        #endregion TableFieldsListRefresh

        #region TableRowDelete
        public Boolean TableRowDelete(DataSet ds, int TableNo)
        {
            Boolean sonuc = true;

            if (ds == null) return sonuc;
            if (ds.Tables.Count == 0) return sonuc;
            if (ds.Tables[TableNo].Rows.Count == 0) return sonuc;
            try
            {
                for (int i = ds.Tables[TableNo].Rows.Count - 1; i >= 0; i--)
                {
                    ds.Tables[TableNo].Rows[i].Delete();
                }
                ds.Tables[TableNo].AcceptChanges();
            }
            catch (Exception e)
            {
                sonuc = false;
                MessageBox.Show("HATALI : " + v.ENTER2 + e.Message, "TableRemove");
            }

            return sonuc;
        }
        #endregion TableRowDelete

        #region TableClean
        public Boolean TableClean(Form tForm, DataSet ds)
        {
            Boolean sonuc = true;

            // Table_Type = 1.Table olmalı
            if (ds == null) return sonuc;
            if (ds.Tables.Count == 0) return sonuc;

            string myProp = ds.Namespace.ToString();
            string TableLabel = MyProperties_Get(myProp, "TableLabel:");
            string Kisitlama = MyProperties_Get(myProp, "Kisitlama:");
            string Sql = MyProperties_Get(myProp, "SqlFirst:");

            if (IsNotNull(Kisitlama))
            {
                #region  
                int ga = Sql.IndexOf("/*KISITLAMALAR|1|*/");
                if (ga == -1)
                {
                    int i_bgn = Sql.IndexOf("/*" + TableLabel + ".DEFAULT_VALUE*/") + 20 + TableLabel.Length; //18 + 2;

                    Sql = Sql.Insert(i_bgn, Kisitlama);
                }

                try
                {
                    string TableIPCode = Set(ds.DataSetName.ToString(), "", "");

                    Control cntrl = new Control();
                    cntrl = Find_Control_View(tForm, TableIPCode);

                    Data_Read_Execute(tForm, ds, ref Sql, "", null);

                    // her halükarda viev kullanılı hale gelsin
                    if (cntrl != null) cntrl.Enabled = true;

                    sonuc = true;

                }
                catch (Exception)
                {
                    sonuc = false;
                    throw;
                }
                #endregion
            }

            return sonuc;
        }
        #endregion TableClean

        #region TableRowGet
        public bool TableRowGet(Form tForm, string TableIPCode)
        {
            bool sonuc = false;

            v.con_SelectedSearchDataRow = null;

            DataSet ds = null;
            DataNavigator dN = null;

            //ds = Find_DataSet(tForm, "", TableIPCode, "");
            Find_DataSet(tForm, ref ds, ref dN, TableIPCode);

            if (ds == null) return sonuc;

            //int pos = Find_DataNavigator_Position(tForm, TableIPCode);

            if (IsNotNull(ds))
            {
                #region
                try
                {
                    string value = ds.Tables[0].Rows[dN.Position][0].ToString();
                    if (IsNotNull(value) == false)
                    {
                        MessageBox.Show("Dikkat : Lütfen önce kayıt işlemini gerçekleştirin ...");
                        return sonuc;
                    }
                    
                    v.con_SelectedSearchDataRow = ds.Tables[0].Rows[dN.Position];
                    sonuc = true;
                }
                catch (Exception)
                {
                    v.con_SelectedSearchDataRow = null;
                    sonuc = false;
                }
                #endregion
            }

            return sonuc;
        }
        #endregion TableRowGet

        #region TableFieldValueGet
        public int TableKeyFieldValue(Form tForm, string TableIPCode, string workType)
        {
            string value = string.Empty;
            string keyFieldName = "";

            TableIPCode = TableIPCode.Trim();

            DataSet ds = null;
            DataNavigator dN = null;
            Find_DataSet(tForm, ref ds, ref dN, TableIPCode);
            string myProp = ds.Namespace.ToString();
            if (IsNotNull(myProp))
            {
                keyFieldName = MyProperties_Get(myProp, "KeyFName:");
                if (IsNotNull(ds))
                    value = ds.Tables[0].Rows[dN.Position][keyFieldName].ToString();
            }
            if (workType == "before")
            {
                v.tTableKeyFieldValue.ds = ds;
                v.tTableKeyFieldValue.dN = dN;
                v.tTableKeyFieldValue.keyFieldName = keyFieldName;
                v.tTableKeyFieldValue.beforeKeyValue = myInt32(value);
            }
            if (workType == "after")
            {
                v.tTableKeyFieldValue.afterKeyValue = myInt32(value);

                checkedTableKeyValue();
            }
            if (workType == "") { } // workType boş ise sadece okunan value gönderiliyor

            return myInt32(value);
        }
        public string TableFieldValueGet(Form tForm, string TableIPCode, string FieldName)
        {
            string value = string.Empty;

            TableIPCode = TableIPCode.Trim();

            DataSet ds = Find_DataSet(tForm, "", TableIPCode, "");
            if (ds == null) return value;

            int pos = Find_DataNavigator_Position(tForm, TableIPCode);

            if (IsNotNull(ds))
            {
                #region
                // fieldname boş ise row isteniyor
                if (FieldName != "")
                {
                    try
                    {
                        value = ds.Tables[0].Rows[pos][FieldName].ToString();
                    }
                    catch (Exception)
                    {
                        value = string.Empty;
                    }
                }
                #endregion
            }

            return value;
        }

        public void checkedTableKeyValue()
        {
            /// table refresh olduktan sonra
            /// dn.Position set ediliyor
            /// bazen pos doğru olsada keyValue aynı olmuyor
            /// bunun nedeni yeni kayıt yapılıyor position en son satırda, sonra refresh oluyor. 
            /// refresh olduğunda order by yüzünden position ve value konumları birbirini tutmuyor
            /// yani position hatalı konuma düşüyor çünkü keyvalue başka positiona taşındı
            /// biz şimdi bu keyvalue şimdi hangi positionda onu bulacağız
            /// 

            /// Eğer refresh öncesi value ile refresh sonrası value aynı ise positionda değişme yok demektir
            /// herhangi bir işleme gerek yoktur
            /// 
            v.tTableKeyFieldValue.beforeKeyValue = 0;
            if (v.tTableKeyFieldValue.beforeKeyValue ==
                   v.tTableKeyFieldValue.afterKeyValue) return;

            int length = v.tTableKeyFieldValue.ds.Tables[0].Rows.Count;
            int pos = 0;
            int readValue = -1;
            for (int i = 0; i < length; i++)
            {
                readValue = myInt32(v.tTableKeyFieldValue.ds.Tables[0].Rows[i][v.tTableKeyFieldValue.keyFieldName].ToString());
                if (readValue == v.tTableKeyFieldValue.afterKeyValue) break;
                pos++;
            }

            v.tTableKeyFieldValue.dN.Position = pos;

        }
        
        #endregion TableFieldValueGet

        #region preparingLocalDbConnectionText
        public bool preparingLocalDbConnectionText()
        {
            bool onay = false;
            string password = "";
            try
            {
                if (v.active_DB.localPsw != "")
                    password = "Password = " + v.active_DB.localPsw + ";";
                else password = "";

                v.active_DB.localConnectionText =
                        string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                        v.active_DB.localServerName,
                        v.active_DB.localDBName,
                        v.active_DB.localUserName,
                        password);

                v.active_DB.localMSSQLConn = new SqlConnection(v.active_DB.localConnectionText);

                v.active_DB.projectServerName = v.active_DB.localServerName;
                v.active_DB.projectDBName = v.active_DB.localDBName;
                v.active_DB.projectUserName = v.active_DB.localUserName;
                v.active_DB.projectPsw = password;
                v.active_DB.projectConnectionText = v.active_DB.localConnectionText;
                v.active_DB.projectMSSQLConn = new SqlConnection(v.active_DB.projectConnectionText);


            }
            catch (Exception)
            {
                onay = false;
                //throw;
            }

            return onay;
        }

        #endregion preparingLocalDbConnectionText

        #region DBConnectState

        public void DBConnectStateFirmPeriod(object sender, StateChangeEventArgs e)
        {
            v.SP_ConnBool_FirmPeriod = (e.CurrentState == ConnectionState.Open);

            if (v.SP_ConnBool_FirmPeriod == false)
                v.Kullaniciya_Mesaj_Var = "DİKKAT : MxSQL Database bağlantısı koptu...";
        }

        public void DBConnectStateFirmMain(object sender, StateChangeEventArgs e)
        {
            v.SP_ConnBool_FirmMain = (e.CurrentState == ConnectionState.Open);

            if (v.SP_ConnBool_FirmMain == false)
                v.Kullaniciya_Mesaj_Var = "DİKKAT : MxSQL Database bağlantısı koptu...";
        }

        public void DBConnectStateProject(object sender, StateChangeEventArgs e)
        {
            v.SP_ConnBool_Project = (e.CurrentState == ConnectionState.Open);

            if (v.SP_ConnBool_Project == false)
                v.Kullaniciya_Mesaj_Var = "DİKKAT : MsSQL Database bağlantısı koptu...";

            if (v.SP_ConnBool_Project != v.SP_ConnBool_Project_Old)
            {
                v.Kullaniciya_Mesaj_Var = "Project bağlantı değişikliği : " + v.SP_ConnBool_Project.ToString();
                v.timer_Kullaniciya_Mesaj_Var_.Enabled = true;
            }

            v.SP_ConnBool_Project_Old = v.SP_ConnBool_Project;
        }

        public void DBConnectStateManager(object sender, StateChangeEventArgs e)
        {
            v.SP_ConnBool_Manager = (e.CurrentState == ConnectionState.Open);

            if (suppressManagerConnWarning)
            {
                v.SP_ConnBool_Manager_Old = v.SP_ConnBool_Manager;
                return;
            }

            if (v.SP_ConnBool_Manager != v.SP_ConnBool_Manager_Old)
            {
                v.SP_Conn_Caption = " " + v.active_DB.managerServerName + " . " + v.active_DB.managerDBName + " ";
                if (v.SP_ConnBool_Manager)
                {
                    v.SP_Conn_Caption = "SQL : " + v.SP_Conn_Caption;
                }
                else
                {
                    v.SP_Conn_Caption = "Bağlantı Yok : " + v.SP_Conn_Caption;

                    if ((v.SP_ConnBool_Manager == false) && (v.SP_ConnBool_Manager_Old))
                    {
                        MessageBox.Show("DİKKAT : Server ve Database bağlantı sorunu..." + v.ENTER2 +
                          "Server Name :  " + v.active_DB.managerServerName + "  " + v.ENTER +
                          "Database Name :  " + v.active_DB.managerDBName + "  ", "Önemli Uyarı");
                    }
                }

                if (v.SP_ConnBool_Manager != v.SP_ConnBool_Manager_Old)
                {
                    v.Kullaniciya_Mesaj_Var = "ManagerServer bağlantı değişikliği : " + v.SP_ConnBool_Manager.ToString();
                    v.timer_Kullaniciya_Mesaj_Var_.Enabled = true;
                }

                v.SP_ConnBool_Manager_Old = v.SP_ConnBool_Manager;

            } // if
        }

        #endregion DBConnectState

        #region ViewControl_Enabled

        public void ViewControl_Enabled_ExtarnalIP(Form tForm, DataSet dsData)
        {
            ///= External_IP:True;
            ///= External_TableIPCode:HPFNS.HPFNS_KS02;
            ///
            string TableIPCode = "";
            string myProp = dsData.Namespace.ToString();

            /// External_IP=True
            /// bir ıp ye birden fazla extra_IP bağlı olabilir 

            if (myProp.IndexOf("External_IP:True") > -1)
            {

                while (myProp.IndexOf("External_TableIPCode:") > -1)
                {
                    TableIPCode = MyProperties_Get(myProp, "External_TableIPCode:");

                    ViewControl_Enabled(tForm, dsData, TableIPCode);

                    Get_And_Clear(ref myProp, "External_TableIPCode:");
                }

            }

        }
        public void ViewControl_Enabled(Form tForm, string TableIPCode, bool value)
        {
            Control cntrl = null;
            cntrl = Find_Control_View(tForm, TableIPCode);
            if (cntrl != null)
                cntrl.Enabled = value;
        }
        public void ViewControl_Enabled(Form tForm, DataSet dsData, string TableIPCode) //Control cntrl)
        {
            // Control Enabled
            Control cntrl = null;// new Control();
            cntrl = Find_Control_View(tForm, TableIPCode);
            if (cntrl != null)
            {
                vTableAbout vTA = new vTableAbout();
                Table_About(vTA, dsData);

                // RecordCount aslında rowCount u verir
                // rowCount varsa enabled=true  onun  harici enabled=false olacak
                if (vTA.RecordCount > 0)
                {
                    //if (TableIPCode == "HPFNS.HPFNS_BN01")
                    //MessageBox.Show("ViewControl_Enabled : " + cntrl.GetType());

                    if (cntrl.GetType().ToString() == "DevExpress.XtraScheduler.SchedulerControl")
                    {
                        string startDateFieldName = ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DataStorage.Appointments.Mappings.Start.ToString();
                        //MessageBox.Show("ViewControl_Enabled : " + startDateFieldName);
                        if (startDateFieldName != "")
                        {
                            //Control tEdit1 = Find_Control(tForm, "spinEdit1_" + TableIPCode);

                            int hafta = 2; // Convert.ToInt32(((DevExpress.XtraEditors.SpinEdit)tEdit1).EditValue);

                            // Start date of the first week
                            DateTime startDate = Convert.ToDateTime(dsData.Tables[0].Rows[0][startDateFieldName].ToString()); 
                            DateTime endDate = startDate.AddDays(7*hafta);

                            //object tDataTable = ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DataStorage.Appointments.DataSource.DataSource;
                            //DataSet dsData = ((DataTable)tDataTable).DataSet;
                            ((DevExpress.XtraScheduler.SchedulerControl)cntrl).BeginInit();
                            ((DevExpress.XtraScheduler.SchedulerControl)cntrl).Start = startDate;

                            //((DevExpress.XtraScheduler.SchedulerControl)cntrl).Views.FullWeekView.SetVisibleIntervals(new TimeInterval(startDate, endDate));

                            try
                            {
                                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).EndInit();
                                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DataStorage.RefreshData();
                                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).RefreshData();
                            }
                            catch (Exception)
                            {
                                //throw;
                            }
                            
                            Object dataSetx = new Object();
                            dataSetx = ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DataStorage.Appointments.DataSource;

                            if (((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType == DevExpress.XtraScheduler.SchedulerViewType.Day)
                            {
                                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DayView.TimeRulers.Add(v.timeRuler);
                                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DayView.TimeRulers[0].Visible = true;
                            }

                        }
                    }

                    cntrl.Enabled = true;
                }
                else
                {
                    string myProp = dsData.Namespace.ToString();
                    string AutoInsert = MyProperties_Get(myProp, "AutoInsert:");

                    if (AutoInsert == "True")
                    {
                        tEvents ev = new tEvents();
                        DataNavigator dN = Find_DataNavigator(tForm, TableIPCode);
                        ev.tSubWork_Refresh_(tForm, dsData, dN);
                    }

                    /*
                    cntrl.Enabled = false;

                    Control btn = null;
                    string[] controls = new string[] { };
                    btn = Find_Control(tForm, "simpleButton_yeni_hesap", TableIPCode, controls);

                    if (btn != null)
                    {
                        if (((DevExpress.XtraEditors.SimpleButton)btn).Visible)
                        {
                            if (((DevExpress.XtraEditors.SimpleButton)btn).Tag != null)
                                ((DevExpress.XtraEditors.SimpleButton)btn).Text =
                                    ((DevExpress.XtraEditors.SimpleButton)btn).Tag.ToString();
                        }
                    }
                    */
                }
            }
        }

        #endregion

        #region DataRowCopy

        public DataRow DataRowCopy(DataSet dsData, int position, string Key_FieldName)
        {
            DataRow New_Row = null;
            int ColumnCount = dsData.Tables[0].Columns.Count;

            New_Row = dsData.Tables[0].NewRow();

            for (int i = 0; i < ColumnCount; i++)
            {
                if (dsData.Tables[0].Columns[i].ColumnName != Key_FieldName)
                {
                    try
                    {
                        New_Row[i] = dsData.Tables[0].Rows[position][i];
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }

            return New_Row;
        }

        #endregion DataRowCopy
        
        #region TableRefresh

        public Boolean TableRefresh(Form tForm, DataSet ds, string TableIPCode)
        {
            Boolean sonuc = true;

            sonuc = TableRefresh(tForm, ds);

            //ButtonEnabledAll(tForm, TableIPCode, true);

            return sonuc;
        }

        public bool TableRefresh(Form tForm, DataSet ds)
        {
            bool sonuc = true;

            // Table_Type = 1.Table olmalı
            if (ds == null) return false;
            //if (ds.Tables.Count == 0) return sonuc;
            //if (IsNotNull(ds) == false) return sonuc;
            string softwareCode = "";
            string projectCode = "";
            string TableCode = "";
            string IPCode = "";

            string myProp = ds.Namespace.ToString();
            string TableIPCode = Set(ds.DataSetName.ToString(), "", "");
            TableIPCode_Get(TableIPCode, ref softwareCode, ref projectCode, ref TableCode, ref IPCode);

            DataNavigator dN = Find_DataNavigator(tForm, TableIPCode);
            int pos = dN.Position;
            /// valeu değişikliği sağlanıyor : -1 > 1234  oluyor
            TableRefreshNewValue(tForm, ds, "Refresh", pos);

            /// myProp u tekrar okunması gerekiyor sakın silme ( TableRefreshNewValue de değşime uğruyor)
            myProp = ds.Namespace.ToString();
            string SqlFirst = MyProperties_Get(myProp, "SqlFirst:");
            string SqlSecond = MyProperties_Get(myProp, "SqlSecond:");
            string KeyIdValue = MyProperties_Get(myProp, "KeyIdValue:");
            string Sql = Set(SqlSecond, SqlFirst, "");

            if (Sql.IndexOf("-99") > -1)
            {
                tEvents ev = new tEvents();
                
                //DataNavigator dN = Find_DataNavigator(tForm, TableIPCode);
                ev.tSubDetail_Refresh(ds, dN);
                return true;
            }
            
            if (IsNotNull(Sql))
            {
                #region
                try
                {
                    Control cntrl = null;
                    //cntrl = Find_Control_View(tForm, TableIPCode);
                    sonuc = Data_Read_Execute(tForm, ds, ref Sql, IPCode, cntrl);
                   
                    ViewControl_Enabled(tForm, TableIPCode, sonuc);
                }
                catch (Exception)
                {
                    sonuc = false;
                    //throw;
                }
                #endregion
            }

            v.con_Refresh = false;
            return sonuc;
        }

        public bool TableRefresh(Form tForm, string tableIPCode)
        {
            bool onay = false;
            if (IsNotNull(tableIPCode))
            {
                DataSet ds = null;
                DataNavigator dN = null;
                Find_DataSet(tForm, ref ds, ref dN, tableIPCode);
                if (ds != null)
                {
                    v.con_PositionChange = true;
                    int pos = dN.Position;
                    TableRefresh(tForm, ds);
                    dN.Position = pos;
                    v.con_PositionChange = false;
                }
                /*
                DataSet ds = Find_DataSet(tForm, "", tableIPCode, "");

                if (ds != null)
                {
                    /// sadece kendisi refresh olsun 
                    /// kendisine bağlı olanlar refresh olmasın
                    /// dataNavigator_PositionChanged( 
                    ///
                    v.con_Cancel = true;

                    onay = TableRefresh(tForm, ds);
                }
                */
            }
            return onay;
        }

        public bool checkedKeyFieldValueNegative(DataSet dsTarget)
        {
            bool onay = true;

            string myProp = dsTarget.Namespace.ToString();
            //string TableIPCode = MyProperties_Get(myProp, "TableIPCode:");
            string TableLabel = MyProperties_Get(myProp, "TableLabel:");
            string KeyFName = MyProperties_Get(myProp, "KeyFName:");
            //string IsUseNewRefId = MyProperties_Get(myProp, "IsUseNewRefId:");
            string KeyIdValue = MyProperties_Get(myProp, "KeyIdValue:");
            //string SqlF = MyProperties_Get(myProp, "SqlFirst:");
            string SqlS = MyProperties_Get(myProp, "SqlSecond:");

            // -1 varmı kontrol edilecek
            KeyIdValue = "-1";
            string UseOldRefId = " and " + TableLabel + "." + KeyFName + " = " + KeyIdValue + " ";

            if (SqlS.IndexOf(UseOldRefId) > -1)
                onay = false;

            return onay;
        }

        public void changeKeyFieldValue(DataSet dsTarget, string newValue)
        {
            string myProp = dsTarget.Namespace.ToString();
            //string TableIPCode = MyProperties_Get(myProp, "TableIPCode:");
            string TableLabel = MyProperties_Get(myProp, "TableLabel:");
            string KeyFName = MyProperties_Get(myProp, "KeyFName:");
            //string IsUseNewRefId = MyProperties_Get(myProp, "IsUseNewRefId:");
            string KeyIdValue = MyProperties_Get(myProp, "KeyIdValue:");
            //string SqlF = MyProperties_Get(myProp, "SqlFirst:");
            string SqlS = MyProperties_Get(myProp, "SqlSecond:");
            string SqlSOld = SqlS;

            if (KeyIdValue == newValue) return;

            string UseOldRefId = " and " + TableLabel + "." + KeyFName + " = " + KeyIdValue + " ";

            //and [BStokB.Id] = -1
            //and BStokB.Id = -1
            if (SqlS.IndexOf(UseOldRefId) == -1)
            {
                TableLabel = TableLabel.Replace("[", "");
                TableLabel = TableLabel.Replace("]", "");
                UseOldRefId = " and " + TableLabel + "." + KeyFName + " = " + KeyIdValue + " ";
            }

            string UseReadRefId = " and " + TableLabel + "." + KeyFName + " = " + newValue + " ";

            Str_Replace(ref SqlS, UseOldRefId, UseReadRefId);

            Str_Replace(ref myProp, "SqlSecond:" + SqlSOld, "SqlSecond:" + SqlS);
            Str_Replace(ref myProp, "KeyIdValue:" + KeyIdValue, "KeyIdValue:" + newValue);
            dsTarget.Namespace = myProp;
        }

        public bool TableRefreshNewValue(Form tForm, DataSet dsTarget, string newValue, int pos)
        {
            bool onay = true;

            //string targetTableIpCode = item.TABLEIPCODE;
            string myProp = dsTarget.Namespace.ToString();
            string TableIPCode = MyProperties_Get(myProp, "TableIPCode:");
            string TableLabel = MyProperties_Get(myProp, "TableLabel:");
            string TableLabel2 = TableLabel.Replace("[", "");
            TableLabel2 = TableLabel2.Replace("]", ""); // [xxx] parentezleri temizle xxx kalsın
            string KeyFName = MyProperties_Get(myProp, "KeyFName:");
            string IsUseNewRefId = MyProperties_Get(myProp, "IsUseNewRefId:");
            string KeyIdValue = MyProperties_Get(myProp, "KeyIdValue:");
            string SqlF = MyProperties_Get(myProp, "SqlFirst:");
            string SqlS = MyProperties_Get(myProp, "SqlSecond:");
            string SqlSOld = SqlS;
            bool run = true;

            if (newValue == "Refresh" && KeyIdValue == "-1" && pos > -1)
            {
                // keyField varmı diye kontrol et
                if (dsTarget.Tables[0].Columns.Contains(KeyFName))
                {
                    newValue = dsTarget.Tables[0].Rows[pos][KeyFName].ToString();
                    if (newValue == "") newValue = "-1";
                    // burada Execute çalışmasın, döndüğü yerde zaten çalışıyor boş yere 2 defa çalışmasın
                    run = false;
                }
            }
            if (newValue == "Refresh" && (KeyIdValue != "-1" || pos == -1))
            {
                return true;
            }

            string UseOldRefId = " and " + TableLabel + "." + KeyFName + " = " + KeyIdValue + " ";
            string UseOldRefId2 = " and " + TableLabel2 + "." + KeyFName + " = " + KeyIdValue + " ";
            string UseReadRefId = " and " + TableLabel + "." + KeyFName + " = " + newValue + " ";


            /// [MtskAday2].Id = -1 yok fakat   MtskAday2.Id = -1  varsa
            if (SqlS.IndexOf(UseOldRefId) == -1 && SqlS.IndexOf(UseOldRefId2) > -1)
                UseOldRefId = UseOldRefId2;

            /// Listeden  Kart açtığınızde KeyIdValue = -1 fakat SqlS içinde ise okuduğu kartın gerçek değeri var örn : and [MtskAday2].Id = 3318
            /// UseOldRefId ise '  and [MtskAday2].Id = -1  ' şeklinde
            /// Yeni gelen newValueyi set edemiyoruz
            /// bunu çözelim
            if (SqlS.IndexOf(UseOldRefId) == -1)
            {
                if (dsTarget.Tables[0].Columns.Contains(KeyFName))
                {
                    /// KeyIdValue = -1  zaten bu değeri taşıyor onun için gerçekteki value değerini okuyalım
                    /// 
                    if (pos == -1)
                    {
                        DataNavigator dN = Find_DataNavigator(tForm, TableIPCode);
                        if (dN != null) pos = dN.Position;
                    }
                    if (pos > -1)
                    {
                        KeyIdValue = dsTarget.Tables[0].Rows[pos][KeyFName].ToString();
                        if (KeyIdValue == "") KeyIdValue = "-1";
                        /// yeniden hazırlayalım
                        UseOldRefId = " and " + TableLabel + "." + KeyFName + " = " + KeyIdValue + " ";
                        if (SqlS.IndexOf(UseOldRefId) == -1)
                            UseOldRefId = " and " + TableLabel + "." + KeyFName + " = " + KeyIdValue;
                    }
                }
            }
                        
            Str_Replace(ref SqlS, UseOldRefId, UseReadRefId);

            Str_Replace(ref myProp, "SqlSecond:" + SqlSOld, "SqlSecond:" + SqlS);
            Str_Replace(ref myProp, "KeyIdValue:" + KeyIdValue, "KeyIdValue:" + newValue);
            dsTarget.Namespace = myProp;
            //onay = TableRefresh(tForm, dsTarget);
            
            if (IsNotNull(SqlS) && (run))
            {
                try
                {
                    Control cntrl = null;
                    //cntrl = Find_Control_View(tForm, TableIPCode);
                    onay = Data_Read_Execute(tForm, dsTarget, ref SqlS, "", cntrl);
                    ViewControl_Enabled(tForm, TableIPCode, onay);
                }
                catch (Exception)
                {
                    onay = false;
                    //throw;
                }
            }
            
            return onay;
        }



        #endregion TableRefresh

        #region TriggerOnOff

        //DISABLE TRIGGER Person.uAddress ON Person.Address;
        //ENABLE Trigger Person.uAddress ON Person.Address;
        public bool tTriggerOnOff(string TableName, string TriggerName, v.tEnabled enabled)
        {
            Boolean onay = false;
            try
            {
                SqlCommand cmd = new SqlCommand(@"@_Enabled TRIGGER @_TriggerName ON @_TableName; ");

                if (enabled == v.tEnabled.Enable)
                    cmd.Parameters.AddWithValue("@_Enabled", "ENABLE");
                else cmd.Parameters.AddWithValue("@_Enabled", "DISABLE");

                cmd.Parameters.AddWithValue("@_TriggerName", TriggerName);
                cmd.Parameters.AddWithValue("@_TableName", TableName);

                string myProp = string.Empty;
                MyProperties_Set(ref myProp, "DBaseNo", "4");
                MyProperties_Set(ref myProp, "TableName", TableName);
                MyProperties_Set(ref myProp, "SqlFirst", "null");
                MyProperties_Set(ref myProp, "SqlSecond", "null");

                vTable vt = new vTable();
                preparing_vTable(null, myProp, vt, 0);
                Sql_ExecuteNon(cmd, vt);
                onay = true;
                cmd.Dispose();
            }
            catch (SqlException se)
            {
                throw se;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return onay;
        }

        #endregion TriggerOnOff

        #region runScript
        public bool runScript(v.dBaseNo targetDBaseNo, string cumle)
        {
            bool onay = false;

            /// Sql execute sırasında script kontrolünden geçmeyecek
            ///
            v.con_CreateScriptPacket = true;

            DataSet ds = new DataSet();
            try
            {
                onay = SQL_Read_Execute(targetDBaseNo, ds, ref cumle, "TABLE1", null);
            }
            catch (Exception)
            {
                throw;
            }
            
            ds.Dispose();
            v.con_CreateScriptPacket = false;

            return onay;
        }
        #endregion

        #region RunQueryModels
        public async Task<List<T>> RunQueryModelsAsync<T>(DataSet ds)
        {
            //DataTable dataTable = await RunQueryTableAsync(queryAbout);
            //table.Namespace = "UstadErrorTable";
            DataTable dataTable = ds.Tables[0];

            // DataTable ile okunan datayı model class a atayalım 
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            List<T> packet = JsonConvert.DeserializeObject<List<T>>(json);

            return packet;
        }

        public async Task<T> RunQueryModelsAsyncSingle<T>(DataSet ds)
        {
            //DataTable dataTable = await RunQueryTableAsync(queryAbout);
            //table.Namespace = "UstadErrorTable";
            DataTable dataTable = ds.Tables[0];

            // DataTable ile okunan datayı model class a atayalım 
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            T packet = JsonConvert.DeserializeObject<T>(json);

            return packet;
        }

        public List<T> RunQueryModels<T>(DataSet ds)
        {
            if (ds == null) return null;
            //DataTable dataTable = await RunQueryTableAsync(queryAbout);
            //table.Namespace = "UstadErrorTable";
            DataTable dataTable = ds.Tables[0];


            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    // Hataları loglamak istersen buraya yazabilirsin
                    args.ErrorContext.Handled = true;
                }
            };

            // DataTable ile okunan datayı model class a atayalım 
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            List<T> packet = JsonConvert.DeserializeObject<List<T>>(json, settings);

            return packet;
        }

        public List<T> RunQueryModelsSingle<T>(DataSet ds, int pos)
        {
            //DataTable dataTable = await RunQueryTableAsync(queryAbout);
            //table.Namespace = "UstadErrorTable";
            
            DataRow row = ds.Tables[0].Rows[pos];
            DataTable dataTable = ds.Tables[0].Clone();
                        
            DataRow nrow = dataTable.NewRow();
            for (int i = 0;  i < row.ItemArray.Length; i++)
            {
                nrow[i] = row[i]; 
            }
            dataTable.Rows.Add(nrow);

            // DataTable ile okunan datayı model class a atayalım 
            //string json = JsonConvert.SerializeObject(ds.Tables[0].Rows[pos], Formatting.Indented); çalışmadı
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            List<T> packet = JsonConvert.DeserializeObject<List<T>>(json);

            return packet;
        }

        #endregion RunQueryModels

        #region lockFName, lockFieldName kontrolleri
        /// <summary>
        /// IsLock fonksiyomları
        /// </summary>
        public bool onlyChanged_Lkp_Onay_Field(DataSet dsData, int position)
        {
            if (IsNotNull(dsData) == false) return false;
            if (position < 0) return false;
            int colCount = 0;
            DataRow row = null;
            try
            {
                colCount = dsData.Tables[0].Columns.Count;
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                row = dsData.Tables[0].Rows[position];
            }
            catch (Exception)
            {
                return false;
            }       
            
            string fieldName = "";
            string orjValue = "";
            string curValue = "";
            bool onay = false;
            bool onay_LpkOnayFieldChanged = false;
            bool onay_OrderFieldsChanged = false;

            for (int i = 0; i < colCount; i++)
            {
                fieldName = dsData.Tables[0].Columns[i].ColumnName;
                try
                {
                    orjValue = row[fieldName, DataRowVersion.Original].ToString();
                }
                catch (Exception)
                {
                    orjValue = "tError";
                }
                try
                {
                    curValue = row[fieldName, DataRowVersion.Current].ToString();
                }
                catch (Exception)
                {
                    //throw;
                }

                // 
                if (orjValue != "tError")
                {
                    if (fieldName == "LKP_ONAY")
                        if (orjValue != curValue) onay_LpkOnayFieldChanged = true;
                    if (fieldName != "LKP_ONAY")
                        if (orjValue != curValue) onay_OrderFieldsChanged = true;
                }
            }

            if ((onay_LpkOnayFieldChanged == true) && (onay_OrderFieldsChanged == false))
                onay = true;

            return onay;
        }

        public bool checkedIsLockRow(DataSet dsData, int position)
        {
            /// v.con_FieldLockControl = True ise contrrol et değilse dön
            /// Mebbisden data alırken Lock kontrolünün olmaması gerekiyor
            /// 
            if (v.con_FieldLockControl == false) 
                return false;
            
            string myProp = dsData.Namespace;
            string lockFieldName = MyProperties_Get(myProp, "LockFName:");

            /// LockField ekran üzerinden kullanıcı tarafından değiştirilemiyor
            /// Ne true > false, ne de false > true ye çevrilebiliyor
            /// 

            /// IsLock Kontrolü yapılıyor
            ///
            if (IsNotNull(lockFieldName))
            {
                //bool isLockRecord = false;

                string lockValue = "";
                string orjValue = "";

                try
                {
                    orjValue = dsData.Tables[0].Rows[position][lockFieldName, DataRowVersion.Original].ToString();
                }
                catch (Exception)
                {
                    orjValue = "tError";
                    //throw;
                }
                try
                {
                    lockValue = dsData.Tables[0].Rows[position][lockFieldName].ToString();
                }
                catch (Exception)
                {
                    //throw;
                }

                if (orjValue != "tError")
                {
                    if (orjValue == "True")// && lockValue == "False")
                    {
                        CancelChangeValues(dsData, position, lockFieldName);

                        v.Kullaniciya_Mesaj_Var = "Bu kayıt kilitli ... Değiştirilemez.";
                        return true;
                    }
                }
                else
                {
                    v.Kullaniciya_Mesaj_Var = "DİKKAT : Kilit kontrolünde hata oluşuyor...";
                }
            }

            return false;
        }

        private void CancelChangeValues(DataSet dsData, int position, string lockFieldName)
        {
            int colCount = dsData.Tables[0].Columns.Count;

            string fieldName = "";
            DataRow row = dsData.Tables[0].Rows[position];
            string orjValue = "";
            string curValue = "";
            string value = "";
            bool onay = false;

            for (int i = 0; i < colCount; i++)
            {
                fieldName = dsData.Tables[0].Columns[i].ColumnName;
                try
                {
                    orjValue = row[fieldName, DataRowVersion.Original].ToString();
                }
                catch (Exception)
                {
                    orjValue = "tError";
                    //throw;
                }
                try
                {
                    curValue = row[fieldName, DataRowVersion.Current].ToString();
                }
                catch (Exception)
                {
                    //throw;
                }
                                
                value = row[fieldName].ToString();
                
                //if (!row[fieldName, DataRowVersion.Original].Equals(row[fieldName, DataRowVersion.Current])) 
                if ((orjValue != "tError") &&
                    (!orjValue.Equals(curValue) ||
                     !orjValue.Equals(value)))
                {
                    //orjValue = row[fieldName, DataRowVersion.Original].ToString();
                    //curValue = row[fieldName, DataRowVersion.Current].ToString();
                    row[fieldName] = orjValue;
                    onay = true;
                }

                /// lockFieldName manuel değiştirilmesin
                /// 
                if (lockFieldName == fieldName && orjValue != "tError")
                {
                    row[fieldName] = orjValue;
                    onay = true;
                }

            }
            if (onay)
                dsData.Tables[0].Rows[position].AcceptChanges();
        }

        private bool FalseChangeValues(DataSet dsData, int position, string lockFieldName)
        {
            int colCount = dsData.Tables[0].Columns.Count;

            DataRow row = dsData.Tables[0].Rows[position];
            string orjValue = "";
            string curValue = "";
            string value = "";
            bool onay = false;

            orjValue = row[lockFieldName, DataRowVersion.Original].ToString();
            curValue = row[lockFieldName, DataRowVersion.Current].ToString();
            value = row[lockFieldName].ToString();

            if (!orjValue.Equals(curValue) ||
                !orjValue.Equals(value))
            {
                //orjValue = row[fieldName, DataRowVersion.Original].ToString();
                //curValue = row[fieldName, DataRowVersion.Current].ToString();
                onay = true;
            }

            return onay;
        }


        #endregion lockFName, lockFieldName kontrolleri

        #endregion Database İşlemleri

        #region Firm İşlemleri

        public void getFirmAbout(DataRow row, ref tUstadFirm tFirm)
        {
            //
            tFirm.FirmId = myInt32(row["FirmId"].ToString());
            tFirm.FirmLongName = row["FirmLongName"].ToString();
            tFirm.FirmShortName = row["FirmShortName"].ToString();
            tFirm.FirmGuid = row["FirmGUID"].ToString();
            tFirm.IlKodu = row["CityTypeId"].ToString();
            tFirm.IlceKodu = row["DistrictTypeId"].ToString();

            tFirm.MenuCode = row["MenuCode"].ToString();
            tFirm.SectorTypeId = myInt16(row["SectorTypeId"].ToString());
            tFirm.DatabaseType = "1"; // MSSQL 
            tFirm.DatabaseName = row["DatabaseName"].ToString();
            tFirm.ServerNameIP = row["ServerNameIP"].ToString();
            //tFirm.DbAuthentication = dbAuthentication;
            tFirm.DbLoginName = row["DbLoginName"].ToString();
            tFirm.DbPassword = row["DbPass"].ToString();
            tFirm.DbTypeId = myInt16(row["DbTypeId"].ToString()); // 2 = Abone database (Ustad yazılım müşterileri)
            tFirm.FirmMebbisCode = row["MebbisCode"].ToString();
            tFirm.FirmMebbisPass = row["MebbisPass"].ToString();

            /// şimdilik manuel çözüm yaptım
            /// Tabim localdb ve Tabim yeni projesi ayrışımını çözemedim şu an için
            /// aynı müşteri hem local hemde yeni projeye geçiş yapmış olabilir
            ///
            menuCodeChecked();
        }

        public void setSelectFirm(tUstadFirm tFirm)
        {
            v.tUser.MainFirmId = tFirm.FirmId;
            
            v.SP_FIRM_ID = tFirm.FirmId;
            v.SP_Firm_SectorTypeId = tFirm.SectorTypeId;

            ///(0, N''),
            ///(1, N'Ön Muhasebe'),
            ///(2, N'Mali Müşavir'),
            ///(3, N'Resmi Muhasebe'),
            ///(4, N'Bordro'),
            ///(5, N'Ustad Crm'),
            ///(201, N'Mtsk'),
            ///(202, N'İşmak'),
            ///(203, N'SRC'),
            ///(211, N'TabimMtsk')

            v.active_DB.projectDBName = tFirm.DatabaseName;
            
            //if (v.active_DB.mainManagerDbUses == false)
                v.active_DB.projectServerName = tFirm.ServerNameIP; // "195.xx";

            v.active_DB.projectUserName = tFirm.DbLoginName; // "sa";

            if (tFirm.DbPassword != "")
                v.active_DB.projectPsw = "Password = " + tFirm.DbPassword + ";"; // Password = 1;
            else v.active_DB.projectPsw = "";

            v.active_DB.projectDBType = v.dBaseType.MSSQL;

            v.active_DB.projectConnectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.active_DB.projectServerName,
                v.active_DB.projectDBName,
                v.active_DB.projectUserName,
                v.active_DB.projectPsw);

            v.active_DB.projectMSSQLConn = new SqlConnection(v.active_DB.projectConnectionText);
            v.active_DB.projectMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateProject);
        }

        #endregion Firm İşlemleri

        #region *SQL Syntax

        #region IfExists_Preparing

        public string IfExists_Preparing(string SorguSQL, string SonucVarsa_SQL, string SonucYoksa_SQL)
        {
            string NewSql = string.Empty;

            NewSql = @"
            IF EXISTS ( 
            /*SORGU*/
            " + SorguSQL + @"
            )
            BEGIN
              use " + v.active_DB.projectDBName + @"
              /*VARSA*/ -- print 'varsa'
              " + SonucVarsa_SQL + @" 
            END ELSE BEGIN
              use " + v.active_DB.projectDBName + @"
              /*YOKSA*/ -- print 'yoksa'
              " + SonucYoksa_SQL + @" 
            END ";

            return NewSql;
        }

        #endregion IfExists_Preparing

        #region SQLPreparing

        public bool DeclarePreparingField(string cumle, string value, int ffieldtype)
        {
            bool onay = false;

            if ((cumle.IndexOf("{GUN}") > -1) &&
                (cumle.IndexOf("{gun}") > -1))
            {
                onay = true;
                v.Declare_Gun = tData_Convert(value, ffieldtype);
                return onay;
            }
            if ((cumle.IndexOf("{AY}") > -1) &&
                (cumle.IndexOf("{ay}") > -1))
            {
                onay = true;
                v.Declare_Ay = tData_Convert(value, ffieldtype);
                return onay;
            }
            if ((cumle.IndexOf("{YIL}") > -1) &&
                (cumle.IndexOf("{yil}") > -1))
            {
                onay = true;
                v.Declare_Yil = tData_Convert(value, ffieldtype);
                return onay;
            }

            if (cumle.IndexOf("{bugun}") > -1)
            {
                onay = true;
                v.Declare_bugun = tData_Convert(value, ffieldtype);
                return onay;
            }

            return onay;
        }

        public string SQLPreparing(string Sql, vTable vt)
        {

            // SQL içindeki " işaretleri ' ile değiştirilir
            //Sql = Sql.ToUpper();
            //MessageBox.Show(Sql.ToUpper().IndexOf("INSERT").ToString() + ";" + Sql.ToUpper().IndexOf("UPDATE").ToString());
            //MessageBox.Show(Sql.ToUpper());
            
            //if ((Sql.ToUpper().IndexOf("INSERT") == -1) &&
            //    (Sql.ToUpper().IndexOf("İNSERT") == -1) &&
            //    (Sql.ToUpper().IndexOf("UPDATE") == -1))
            if (v.con_CreateScriptPacket == false)
                Sql = Str_AntiCheck(Sql);

            Sql = Str_CheckYilAy(Sql);

            // Yukarıda toUpper yüzüunden join > JOİN şekline dönüşüyor
            //Str_Replace(ref Sql, "JOİN", "JOIN");
            //Str_Replace(ref Sql, "İSNULL", "ISNULL");
            //Str_Replace(ref Sql, "UNİON", "UNION");

            //if (vt.TableIPCode == "SYSUSER.SYSUSER_L02")
            //{
            //    v.Kullaniciya_Mesaj_Var = vt.TableIPCode;
            //}

            Str_Replace(ref Sql, ":VT_FIRM_ID", v.SP_FIRM_ID.ToString());
            Str_Replace(ref Sql, "\':FIRM_ID\'", v.SP_FIRM_ID.ToString());
            Str_Replace(ref Sql, ":FIRM_ID", v.SP_FIRM_ID.ToString());
            Str_Replace(ref Sql, ":FIRM_ILKODU", v.tMainFirm.IlKodu);
            Str_Replace(ref Sql, ":FIRM_ILCEKODU", v.tMainFirm.IlceKodu);
            Str_Replace(ref Sql, ":FIRM_ILADI", v.tMainFirm.IlAdi);
            Str_Replace(ref Sql, ":FIRM_ILCEADI", v.tMainFirm.IlceAdi);
            Str_Replace(ref Sql, ":FIRM_DBNAME", v.tMainFirm.DatabaseName);
            //Str_Replace(ref Sql, ":FIRM_USERLIST", v.SP_FIRM_USERLIST);
            //Str_Replace(ref Sql, ":FIRM_USER_LIST", v.SP_FIRM_USERLIST);
            //Str_Replace(ref Sql, ":FIRM_FULLLIST", v.SP_FIRM_FULLLIST);
            //Str_Replace(ref Sql, ":FIRM_FULL_LIST", v.SP_FIRM_FULLLIST);

            Str_Replace(ref Sql, ":VT_COMP_ID", v.tComputer.UstadCrmComputerId.ToString()); //v.tComp.SP_COMP_ID.ToString());
            Str_Replace(ref Sql, ":VT_PERIOD_ID", v.vt_PERIOD_ID.ToString());
            Str_Replace(ref Sql, ":VT_USER_ID", v.tUser.UserId.ToString());

            Str_Replace(ref Sql, ":USER_ID", v.tUser.UserId.ToString());
            Str_Replace(ref Sql, ":USER_TCNO", "'" + v.tUser.UserTcNo + "'");
            Str_Replace(ref Sql, ":USER_DBTYPE_ID", v.tUser.UserDbTypeId.ToString());
            Str_Replace(ref Sql, ":USER_GUID", v.tUser.UserGUID);
            Str_Replace(ref Sql, ":USERGUID", v.tUser.UserGUID);
            Str_Replace(ref Sql, ":UserGUID", v.tUser.UserGUID);
            Str_Replace(ref Sql, ":SectorTypeId", v.SP_Firm_SectorTypeId.ToString());
            Str_Replace(ref Sql, ":SECTORTYPEID", v.SP_Firm_SectorTypeId.ToString());


            Str_Replace(ref Sql, ":DONEM_YILAY", v.DONEMTIPI_YILAY.ToString());
            Str_Replace(ref Sql, ":BUGUN_YILAY", v.BUGUN_YILAY.ToString());
            Str_Replace(ref Sql, ":BUGUN_GUN", v.BUGUN_GUN.ToString());
            Str_Replace(ref Sql, ":BUGUN_AY", v.BUGUN_AY.ToString());
            Str_Replace(ref Sql, ":BUGUN_YIL", v.BUGUN_YIL.ToString());
            Str_Replace(ref Sql, ":BUGUN", v.BUGUN_TARIH.ToString());

            Str_Replace(ref Sql, ":GECEN_YILAY", v.GECEN_YILAY.ToString());
            Str_Replace(ref Sql, ":GELECEL_YILAY", v.GELECEK_YILAY.ToString()); // update den sonra kaldır
            Str_Replace(ref Sql, ":GELECEK_YILAY", v.GELECEK_YILAY.ToString());

            Str_Replace(ref Sql, ":FORM_CODE", "'" + vt.FormCode + "'");
            Str_Replace(ref Sql, ":MANAGER_DBNAME", v.active_DB.managerDBName);
            Str_Replace(ref Sql, ":NEWFIRM_DBNAME", v.newFirm_DB.databaseName);


            //Str_Replace(ref Sql, "{gun}", v.BUGUN_GUN.ToString());
            //Str_Replace(ref Sql, "{ay}", v.BUGUN_AY.ToString());
            //Str_Replace(ref Sql, "{yil}", v.BUGUN_YIL.ToString());
            //Str_Replace(ref Sql, "{bugun}", Tarih_Formati(v.BUGUN_TARIH));

            if (v.Declare_Gun != "")
            {
                Str_Replace(ref Sql, "{GUN}", v.Declare_Gun);
                Str_Replace(ref Sql, "{gun}", v.Declare_Gun);
                v.Declare_Gun = string.Empty;
            }
            else
            {
                Str_Replace(ref Sql, "{GUN}", v.BUGUN_GUN.ToString());
                Str_Replace(ref Sql, "{gun}", v.BUGUN_GUN.ToString());
            }

            if (v.Declare_Ay != "")
            {
                Str_Replace(ref Sql, "{AY}", v.Declare_Ay);
                Str_Replace(ref Sql, "{ay}", v.Declare_Ay);
                v.Declare_Ay = string.Empty;
            }
            else
            {
                Str_Replace(ref Sql, "{AY}", v.BUGUN_AY.ToString());
                Str_Replace(ref Sql, "{ay}", v.BUGUN_AY.ToString());
            }

            if (v.Declare_Yil != "")
            {
                Str_Replace(ref Sql, "{YIL}", v.Declare_Yil);
                Str_Replace(ref Sql, "{yil}", v.Declare_Yil);
                v.Declare_Yil = string.Empty;
            }
            else
            {
                Str_Replace(ref Sql, "{YIL}", v.BUGUN_YIL.ToString());
                Str_Replace(ref Sql, "{yil}", v.BUGUN_YIL.ToString());
            }

            if (v.Declare_bugun != "")
            {
                Str_Replace(ref Sql, "{bugun}", v.Declare_bugun);
                v.Declare_bugun = string.Empty;
            }
            else
            {
                Str_Replace(ref Sql, "{bugun}", Tarih_Formati(v.BUGUN_TARIH));
            }

            if (Sql.IndexOf("{baslamaTarihi}") > -1)
            {
                Str_Replace(ref Sql, "{baslamaTarihi}", Tarih_Formati(v.BUGUN_TARIH));
            }

            if (Sql.IndexOf("{bitisTarihi}") > -1)
            {
                Str_Replace(ref Sql, "{bitisTarihi}", Tarih_Formati(v.BUGUN_TARIH));
            }

            if (Sql.IndexOf("[MSV3]") > -1)
            {
                // buraya vt ile mssql mi mysql mi de ileride gerekir
                //

                /*
                if (dBaseNo == 2)
                    Str_Replace(ref Sql, "[MSV3]", "[" + v.active_DB.managerDBName.ToString() + "]");
                if (dBaseNo == 3)
                    Str_Replace(ref Sql, "[MSV3]", "[" + v.db_MAINMANAGER_DBNAME.ToString() + "]");
                */
                Str_Replace(ref Sql, "[MSV3]", "[" + v.active_DB.managerDBName.ToString() + "]");
            }

            // json için de [ ve ]  yerine < ve > kullanılıyor
            if (Sql.IndexOf("<MSV3>") > -1)
            {
                Str_Replace(ref Sql, "<MSV3>", "[" + v.active_DB.managerDBName.ToString() + "]");
            }

            if (Sql.IndexOf("<CRM>") > -1)
            {
                Str_Replace(ref Sql, "<CRM>", "[" + v.active_DB.ustadCrmDBName.ToString() + "]");
            }

            return Sql;
        }

        #endregion SQLPreparing

        #region SQL_Select_Table
        public string SQL_Select_Table(string TableName)
        {
            return " Select * from " + TableName + " where 0 = 0 ";
        }
        #endregion SQL_Select_Table

        #region SQLWhereAdd
        public string SQLWhereAdd(string Sql, string tLabel, string WhereAnd, string Header)
        {
            string tHeader = string.Empty;
            string FullHeader1 = string.Empty;
            string FullHeader2 = string.Empty;
            string FullHeader3 = string.Empty;

            if (Header.IndexOf("DEFAULT") > -1) tHeader = "DEFAULT_VALUE";
            if (Header.IndexOf("KRITER") > -1) tHeader = "KRITERLER";

            FullHeader1 = "/*" + tHeader + "*/";// + v.ENTER;
            FullHeader2 = "/*" + tLabel + "." + tHeader + "*/";// + v.ENTER;
            FullHeader3 = "/*[" + tLabel + "]." + tHeader + "*/";// + v.ENTER;

            if (IsNotNull(WhereAnd))
            {
                /*DEFAULT_VALUE*/
                /*KRITERLER*/
                int f1 = 0;
                int i1 = Sql.IndexOf(FullHeader1, f1);
                while ((i1 > f1) && (f1 > -1))
                {
                    Sql = Sql.Insert((i1 + FullHeader1.Length), v.ENTER + WhereAnd);
                    f1 = Sql.IndexOf(FullHeader1, i1 + FullHeader1.Length);
                    if (f1 > 0)
                    {
                        i1 = f1;
                        f1 = 0;
                    }
                }

                /*[XXX].DEFAULT_VALUE*/
                /*[XXX].KRITERLER*/
                int f2 = 0;
                int i2 = Sql.IndexOf(FullHeader2, f2);
                while ((i2 > f2) && (f2 > -1))
                {
                    Sql = Sql.Insert((i2 + FullHeader2.Length), v.ENTER + WhereAnd);
                    // bulduğunun haricinde başka bir tane daha var mı kontrol et
                    f2 = Sql.IndexOf(FullHeader2, i2 + FullHeader2.Length); 
                    if (f2 > 0)
                    {
                        i2 = f2;
                        f2 = 0;
                    }
                }

                /*XXX.DEFAULT_VALUE*/  
                /*XXX.KRITERLER*/
                /* XXX >> [XXX] haline getiriliyor */
                int f3 = 0;
                int i3 = Sql.IndexOf(FullHeader3, f3);
                while ((i3 > f3) && (f3 > -1))
                {
                    Sql = Sql.Insert((i3 + FullHeader3.Length), v.ENTER + WhereAnd);
                    f3 = Sql.IndexOf(FullHeader3, i3 + FullHeader3.Length);
                    if (f3 > 0)
                    {
                        i3 = f3;
                        f3 = 0;
                    }
                }

            }

            return Sql;
        }
        #endregion SQLWhereAdd

        #endregion SQL Syntax

        #region *Form View

        public Form OpenForm_JSON(Form tForm, PROP_NAVIGATOR prop_)
        {

            //'FORMTYPE',      0,  '',       'none')
            //'FORMTYPE',      0,  'CHILD',  'ChildForm')
            //'FORMTYPE',      0,  'NORMAL', 'NormalForm')
            //'FORMTYPE',      0,  'DIALOG', 'DialogForm')

            //'FORMSTATE',     0,  '',       'none')
            //'FORMSTATE',     0,  'NORMAL', 'Normal')
            //'FORMSTATE',     0,  'MAX',    'Max')
            //'FORMSTATE',     0,  'MIN',    'Min')

            if (prop_ == null)
            {
                MessageBox.Show("Açılmak istenen Formun bilgileri eksik...");
                return null;
            }

            string FORMNAME = Set(prop_.FORMNAME.ToString(), "", "");
            string FORMCODE = Set(prop_.FORMCODE.ToString(), "", "");
            string FORMTYPE = Set(prop_.FORMTYPE.ToString(), "", "");
            string FORMSTATE = Set(prop_.FORMSTATE.ToString(), "", "");

            /// liste ekranları üzerindeki seçilen kaydı açmak için
            /// okunması gereken liste IP leri oku ve 
            /// okunan bu value leri MSETVALUE ye set et
            /// 
            OpenForm_ReadValues_JSON(tForm, prop_.TABLEIPCODE_LIST);

            string myFormLoadValue = JsonConvert.SerializeObject(prop_.TABLEIPCODE_LIST);

            #region 

            // form code var ise boş formu kullanarak Layout dizayn et
            if ((FORMCODE != "") &&
                (FORMNAME == ""))
                FORMNAME = "ms_Form";

            tForms fr = new tForms();

            Form tNewForm = null;

            v.con_FormLoadValue = myFormLoadValue;

            if (IsNotNull(FORMNAME))
            {
                if (Cursor.Current == Cursors.Default)
                    Cursor.Current = Cursors.Hand;

                //v.Kullaniciya_Mesaj_Var = v.DBRec_Insert;

                tNewForm = fr.Get_Form(FORMNAME);

                // MS_LAYOUT Preparing
                // form code var ise boş formu kullanarak Layout dizayn et
                if (FORMCODE != "")
                {
                    tLayout l = new tLayout();
                    l.Create_Layout(tNewForm, FORMCODE);
                }

                if (tNewForm != null)
                {
                    /// ?? nedir 
                    tNewForm.Opacity = 50;
                    //tNewForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                    /// refresh özelliğini taşı
                    /// açılan newForm da kayıt gerçekleşirse on 
                    //tNewForm.HelpButton = tForm.HelpButton;

                    #region
                    if (IsNotNull(FORMTYPE))
                    {
                        if (FORMTYPE == "CHILD")
                        {
                            ChildForm_View(tNewForm, Application.OpenForms[0], myFormLoadValue);
                        }
                        if (FORMTYPE == "NORMAL")
                        {
                            if (FORMSTATE == "NORMAL")
                                NormalForm_View(tNewForm, FormWindowState.Normal, myFormLoadValue);
                            if (FORMSTATE == "MAX")
                                NormalForm_View(tNewForm, FormWindowState.Maximized, myFormLoadValue);
                            if (FORMSTATE == "MIN")
                                NormalForm_View(tNewForm, FormWindowState.Minimized, myFormLoadValue);
                        }
                        if (FORMTYPE == "DIALOG")
                        {
                            if (FORMSTATE == "NORMAL")
                                DialogForm_View(tNewForm, FormWindowState.Normal, myFormLoadValue);
                            if (FORMSTATE == "MAX")
                                DialogForm_View(tNewForm, FormWindowState.Maximized, myFormLoadValue);
                            if (FORMSTATE == "MIN")
                                DialogForm_View(tNewForm, FormWindowState.Minimized, myFormLoadValue);
                        }
                    }
                    else
                    {
                        if (FORMTYPE == "CHILD")
                        {
                            ChildForm_View(tNewForm, Application.OpenForms[0], myFormLoadValue);
                        }
                    }
                    #endregion
                }
                else MessageBox.Show("Form bulunamadı...");

                if (Cursor.Current != Cursors.Default)
                    Cursor.Current = Cursors.Default;

            }

            return tNewForm;
            #endregion
        }

        private void OpenForm_ReadValues_JSON(Form tForm, List<TABLEIPCODE_LIST> TableIPCodeList)
        {
            DataSet ds = null;
            DataNavigator dN = null;

            string WORKTYPE = string.Empty;
            string RTABLEIPCODE = string.Empty;
            string RKEYFNAME = string.Empty;
            string ReadValue = string.Empty;

            foreach (var item in TableIPCodeList)
            {
                WORKTYPE = item.WORKTYPE.ToString();
                RTABLEIPCODE = Set(item.RTABLEIPCODE.ToString(), "", "");
                RKEYFNAME = Set(item.RKEYFNAME.ToString(), "", "");

                #region 1
                if (WORKTYPE == "NEW")
                {
                    ReadValue = "0";
                }
                #endregion

                #region 2
                if (IsNotNull(RTABLEIPCODE) &&
                    IsNotNull(RKEYFNAME) &&
                    ((WORKTYPE == "READ") ||
                     (WORKTYPE == "SVIEW") ||
                     (WORKTYPE == "SVIEWVALUE") ||
                     (WORKTYPE == "CREATEVIEW") ||
                     (WORKTYPE == "OPENFORM") ||
                     (WORKTYPE == "GOTO"))
                    )
                {
                    ds = Find_DataSet(tForm, "", RTABLEIPCODE, "");
                    Find_DataSet(tForm, ref ds, ref dN, RTABLEIPCODE);

                    if (IsNotNull(ds))
                    {
                        ReadValue = ds.Tables[0].Rows[dN.Position][RKEYFNAME].ToString();
                    }
                }
                #endregion

                #region 3
                if (IsNotNull(ReadValue))
                {
                    item.MSETVALUE = ReadValue;
                }
                #endregion
            }
        }


        public void OpenFormPreparing(string FormName, string FormCode, v.formType formType)
        {
            //string FormName = "ms_User";
            //string FormCode = "UST/PMS/PMS/SYS_USERLOGIN";

            string formType_ = "CHILD";

            if (formType == v.formType.Dialog) formType_ = "DIALOG";
            if (formType == v.formType.Normal) formType_ = "NORMAL";
            if (formType == v.formType.Child) formType_ = "CHILD";

            string Prop_Navigator = @"
            0=FORMNAME:" + FormName + @";
            0=FORMCODE:" + FormCode + @";
            0=FORMTYPE:" + formType_ + @";
            0=FORMSTATE:NORMAL;
            ";

            OpenForm(null, Prop_Navigator);
        }

        public void OpenForm(Form tForm, string Prop_Navigator)
        {
            string s1 = "=ROW_PROP_NAVIGATOR:";
            string s2 = (char)34 + "TABLEIPCODE_LIST" + (char)34 + ": [";
            // "TABLEIPCODE_LIST": [

            string s3 = "=TABLEIPCODE_LIST:";
            string s4 = "=FORMCODE:";

            if (Prop_Navigator.IndexOf(s1) > -1)
                OpenForm_OLD(tForm, Prop_Navigator);
            else if (Prop_Navigator.IndexOf(s3) > -1)
                OpenForm_OLD(tForm, Prop_Navigator);
            else if (Prop_Navigator.IndexOf(s4) > -1)
                OpenForm_OLD(tForm, Prop_Navigator);
            else if (Prop_Navigator.IndexOf(s2) > -1)
            {
                Str_Remove(ref Prop_Navigator, "|ds|");

                var prop_ = JsonConvert.DeserializeObject(Prop_Navigator);

                OpenForm_JSON(tForm, (PROP_NAVIGATOR)prop_);
            }
            else
            {
                MessageBox.Show("DİKKAT : Tanımlayamadığım bir PROP_NAVIGATOR cümlesi geldi...");
            }
        }

        private void OpenForm_OLD(Form tForm, string Prop_Navigator)
        {
            #region örnek
            /*
            PROP_NAVIGATOR={
            1=ROW_PROP_NAVIGATOR:1;
            1=CAPTION:KART AÇ;             <<< Hangi butona basıldığına dair bilgi  
            1=BUTTONTYPE:51;
             * 
            1=TABLEIPCODE_LIST:TABLEIPCODE_LIST={   <<<<< field bloğu        <<< okunacak TABLEIPCODE ler
            1=ROW_TABLEIPCODE_LIST:1;               <<<<< row bloğu
            1=CAPTION:Kart aç;
            1=TABLEIPCODE:TSTDTL.TSTDTL_03;
            1=TABLEALIAS:[TSTDTL];
            1=KEYFNAME:MSTR_REF_ID;
            1=RTABLEIPCODE:TSTMSTR.TSTMSTR_01;
            1=RKEYFNAME:REF_ID;
            1=MSETVALUE:null;
            1=WORKTYPE:READ;
            1=ROWE_TABLEIPCODE_LIST:1;
             * 
            2=ROW_TABLEIPCODE_LIST:2;
            2=CAPTION:Cari Hesabı Oku;
            2=TABLEIPCODE:HP_CARI.HP_CARI_02;
            2=TABLEALIAS:[HPCARI];
            2=KEYFNAME:REF_ID;
            2=RTABLEIPCODE:TSTMSTR.TSTMSTR_01;
            2=RKEYFNAME:CARI_ID;
            2=MSETVALUE:null;
            2=WORKTYPE:READ;
            2=ROWE_TABLEIPCODE_LIST:2;
            TABLEIPCODE_LIST=};                 <<<<< 
             * 
            1=FORMNAME:TestFormu1;              <<< açılacak form bilgileri
            1=FORMCODE:null;
            1=FORMTYPE:DIALOG;
            1=FORMSTATE:NORMAL;
            1=ROWE_PROP_NAVIGATOR:1;
            PROP_NAVIGATOR=}
            */
            #endregion örnek

            //'FORMTYPE',      0,  '',       'none')
            //'FORMTYPE',      0,  'CHILD',  'ChildForm')
            //'FORMTYPE',      0,  'NORMAL', 'NormalForm')
            //'FORMTYPE',      0,  'DIALOG', 'DialogForm')

            //'FORMSTATE',     0,  '',       'none')
            //'FORMSTATE',     0,  'NORMAL', 'Normal')
            //'FORMSTATE',     0,  'MAX',    'Max')
            //'FORMSTATE',     0,  'MIN',    'Min')

            string FORMNAME = MyProperties_Get(Prop_Navigator, "FORMNAME:");
            string FORMCODE = MyProperties_Get(Prop_Navigator, "FORMCODE:");
            string FORMTYPE = MyProperties_Get(Prop_Navigator, "FORMTYPE:");
            string FORMSTATE = MyProperties_Get(Prop_Navigator, "FORMSTATE:");


            //MessageBox.Show(FORMCODE + " : için OpenForm_OLD çalışıyor, S.O.S. ");

            string TableIPCode_RowBlock = Find_Properies_Get_FieldBlock(Prop_Navigator, "TABLEIPCODE_LIST");

            TableIPCode_RowBlock = TableIPCodeList_Get_Values_OLD(tForm, TableIPCode_RowBlock);

            string myFormLoadValue = TableIPCode_RowBlock;

            // form code var ise boş formu kullanarak Layout dizayn et
            if ((FORMCODE != "") && (FORMNAME == ""))
                FORMNAME = "ms_Form";

            tForms fr = new tForms();

            Form tNewForm = null;

            v.con_FormLoadValue = myFormLoadValue;

            if (IsNotNull(FORMNAME))
            {
                tNewForm = fr.Get_Form(FORMNAME);

                // MS_LAYOUT Preparing
                // form code var ise boş formu kullanarak Layout dizayn et
                if (FORMCODE != "")
                {
                    tLayout l = new tLayout();
                    l.Create_Layout(tNewForm, FORMCODE);
                }

                if (tNewForm != null)
                {
                    tNewForm.Opacity = 50;
                    //tNewForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                    if (IsNotNull(FORMTYPE))
                    {
                        if (FORMTYPE == "CHILD")
                        {
                            ChildForm_View(tNewForm, Application.OpenForms[0], myFormLoadValue);
                        }
                        if (FORMTYPE == "NORMAL")
                        {
                            if (FORMSTATE == "NORMAL")
                                NormalForm_View(tNewForm, FormWindowState.Normal, myFormLoadValue);
                            if (FORMSTATE == "MAX")
                                NormalForm_View(tNewForm, FormWindowState.Maximized, myFormLoadValue);
                            if (FORMSTATE == "MIN")
                                NormalForm_View(tNewForm, FormWindowState.Minimized, myFormLoadValue);
                        }
                        if (FORMTYPE == "DIALOG")
                        {
                            if (FORMSTATE == "NORMAL")
                                DialogForm_View(tNewForm, FormWindowState.Normal, myFormLoadValue);
                            if (FORMSTATE == "MAX")
                                DialogForm_View(tNewForm, FormWindowState.Maximized, myFormLoadValue);
                            if (FORMSTATE == "MIN")
                                DialogForm_View(tNewForm, FormWindowState.Minimized, myFormLoadValue);
                        }
                    }
                    else
                    {
                        if (FORMTYPE == "CHILD")
                        {
                            ChildForm_View(tNewForm, Application.OpenForms[0], myFormLoadValue);
                        }
                    }
                }
                else MessageBox.Show("Form bulunamadı...");

            }
            //return tNewForm;
        }

        //----
        public void ChildForm_View(Form tForm, Form tMdiForm)
        {
            ChildForm_View(tForm, tMdiForm, string.Empty);
        }

        public void ChildForm_View(Form tForm, Form tMdiForm, string myFormLoadValue)
        {
            // Formun üzerinde MyFormBox isimli memo yok ise eklesin
            tCreateObject co = new tCreateObject();
            co.Create_MyFormBox(tForm, myFormLoadValue);

            v.con_FormOpen = true;
            v.sp_OpenFormState = "CHILD";
            tForm.Tag = v.sp_OpenFormState;
            if (tMdiForm.IsMdiContainer)
                tForm.MdiParent = tMdiForm;
            tForm.Show();
            v.con_FormOpen = false;

            ScreenSize(tForm);
        }

        public void ChildForm_View(Form tForm, Form tMdiForm, FormWindowState state)
        {
            ChildForm_View(tForm, tMdiForm, state, string.Empty);
        }

        public void ChildForm_View(Form tForm, Form tMdiForm, FormWindowState state, string myFormLoadValue)
        {
            // Formun üzerinde MyFormBox isimli memo yok ise eklesin
            tCreateObject co = new tCreateObject();
            co.Create_MyFormBox(tForm, myFormLoadValue);

            if (tMdiForm.IsMdiContainer)
                tForm.MdiParent = tMdiForm;

            v.con_FormOpen = true;
            v.sp_OpenFormState = "CHILD";
            tForm.Tag = v.sp_OpenFormState;
            tForm.WindowState = state;
            tForm.Show();
            v.con_FormOpen = false;

            ScreenSize(tForm);
        }
        //---
        public void NormalForm_View(Form tForm, FormWindowState state)
        {
            NormalForm_View(tForm, state, string.Empty);
        }

        public void NormalForm_View(Form tForm, FormWindowState state, string myFormLoadValue)
        {
            // Formun üzerinde MyFormBox isimli memo yok ise eklesin
            tCreateObject co = new tCreateObject();
            co.Create_MyFormBox(tForm, myFormLoadValue);

            v.con_FormOpen = true;
            v.sp_OpenFormState = "NORMAL";
            tForm.Tag = v.sp_OpenFormState;
            tForm.StartPosition = FormStartPosition.CenterScreen;
            tForm.WindowState = state;
            tForm.Show();
            v.con_FormOpen = false;
        }
        //---
        public void DialogForm_View(Form tForm, FormWindowState state)
        {
            DialogForm_View(tForm, state, string.Empty);
        }

        public void DialogForm_View(Form tForm, FormWindowState state, string myFormLoadValue)
        {
            // Formun üzerinde MyFormBox isimli memo yok ise eklesin
            tCreateObject co = new tCreateObject();
            co.Create_MyFormBox(tForm, myFormLoadValue);

            v.sp_OpenFormState = "DIALOG";

            if (myFormLoadValue == "searchDIALOG")
                v.sp_OpenFormState = myFormLoadValue;

            v.con_FormOpen = true;
            tForm.Tag = v.sp_OpenFormState;
            tForm.TopMost = true;
            tForm.StartPosition = FormStartPosition.CenterScreen;
            tForm.WindowState = state;
            tForm.ShowDialog();
            v.con_FormOpen = false;
        }
        //---
        public void ScreenSize(Form tForm)
        {
            tForm.Top = 0;
            tForm.Left = 0;
            tForm.Width = v.Screen_Width;
            tForm.Height = v.Screen_Height;
        }


        #endregion Form View

        #region JSON işlemleri

        public void readProNavigator(
            string propNavigator,
            ref PROP_NAVIGATOR prop_,
            ref List<PROP_NAVIGATOR> propList_)
        {
            propNavigator = propNavigator.Replace((char)34, (char)39);

            int p1 = propNavigator.IndexOf("[");
            int p2 = propNavigator.IndexOf("Type:SearchEngine;[");
            if (p2 == -1)
                p2 = propNavigator.IndexOf("Type:Buttons;[");

            int len = propNavigator.Length;

            if (((p1 == -1) || (p1 > 10)) && // başlangıçta aray işareti olmaz ve
                (p2 == -1))                  // searchEngine yok ise
            {
                prop_ = readProp<PROP_NAVIGATOR>(propNavigator);
            }
            else
            {
                // varsa sil
                Str_Remove(ref propNavigator, "|ds|");

                propList_ = readPropList<PROP_NAVIGATOR>(propNavigator);
            }
        }

        public T readProp<T>(string prop_)
        {
            T packet = Activator.CreateInstance<T>();

            string s = string.Empty;
            int i = 0;

            s = "=KEY_FNAME:ID;";
            i = prop_.IndexOf(s);

            if (i > -1)
            {
                return default(T);
            }

            
            s = "Type:SearchEngine;";
            i = prop_.IndexOf(s);
            if (i > -1)
                prop_ = prop_.Remove(i, s.Length);

            s = "Type:Buttons;";
            i = prop_.IndexOf(s);
            if (i > -1)
                prop_ = prop_.Remove(i, s.Length);

            /// 34 = " değiştir  39 = '
            prop_ = prop_.Replace((char)34, (char)39);

            //i = prop_.IndexOf("[");
            //if ((i == -1) || (i > 10))
            //    prop_ = "[" + prop_ + "]";
            try
            {
                var propS = JsonConvert.DeserializeAnonymousType(prop_, packet); // dip
                return propS;
            }
            catch (Exception)
            {
                return default(T);
                //throw;
            }
            

            //return default(T);

        }

        public List<T> readPropList<T>(string prop_)
        {
            /// bu fonksiyonda json formatıyla kaydedilmiş string ifadeyi
            /// istenen class a çevirip geri döndürür.
            /// 
            List<T> packet = new List<T>();

            string s = string.Empty;
            int i = 0;

            s = "Type:SearchEngine;";
            while (prop_.IndexOf(s) > -1)
            {
                i = prop_.IndexOf(s);
                prop_ = prop_.Remove(i, s.Length);
            }

            s = "Type:Buttons;";
            while (prop_.IndexOf(s) > -1)
            {
                i = prop_.IndexOf(s);
                prop_ = prop_.Remove(i, s.Length);
            }

            /// 34 = " değiştir  39 = '
            prop_ = prop_.Replace((char)34, (char)39);

            i = prop_.IndexOf("[");
            if ((i == -1) || (i > 10))
                prop_ = "[" + prop_ + "]";

            prop_ = prop_.Replace("|Prop_Navigator|", "");

            try
            {
                var propL = JsonConvert.DeserializeAnonymousType(prop_, packet); // dip
                return propL;
            }
            catch (Exception)
            {
                MessageBox.Show("DİKKAT : JSon paketi kontrol et, sorun mevcut...");
                //throw;
                return null;
            }
        }

        #endregion JSON işlemleri

        #region *String İşlemler

        public string getNewFileGuidName
        {
            get
            {
                string guid = Guid.NewGuid().ToString();
                return guid.Substring(0, 20);
            }
        }

        public string getFileName(v.tFileName tFileName)
        {
            string fileName = "none";
            if (tFileName == v.tFileName.dbKayit) fileName = "DatabaseKayit";
            if (tFileName == v.tFileName.tarayici) fileName = "TarayicidanAlinan";
            if (tFileName == v.tFileName.setImageDPI) fileName = "SetImageDPI";
            if (tFileName == v.tFileName.setImageQuality) fileName = "SetImageQuality";
            if (tFileName == v.tFileName.transferFromDatabaseToWeb) fileName = "TransferFromDatabaseToWeb";
            return fileName;
        }

        public void DosyaVarsaSil(string pathFileName)
        {
            if (File.Exists(pathFileName))
            {
                // Dosya varsa sil
                File.Delete(pathFileName);
            }
        }


        public string sifirlaTamamla(decimal value, int length)
        {
            int intValue = (int)value;
            string paddedValue = intValue.ToString().PadLeft(length, '0');
            return paddedValue;
        }

        #region TurkceKarakterDuzenle
        public string TurkceKarakterDuzenle(string metin)
        {
            // Artı olarak eklenmek ve replace edilmek istenen karakterler olur ise eklenebilir.

            metin = metin.Replace("Ğ", "G"); metin = metin.Replace("ğ", "g");
            metin = metin.Replace("Ü", "U"); metin = metin.Replace("ü", "u");
            metin = metin.Replace("Ş", "S"); metin = metin.Replace("ş", "s");
            metin = metin.Replace("İ", "I"); metin = metin.Replace("ı", "i");
            metin = metin.Replace("Ö", "O"); metin = metin.Replace("ö", "o");
            metin = metin.Replace("Ç", "C"); metin = metin.Replace("ç", "c");

            return metin;
        }

        public string TelefonNumarasiGecerliMi(string telefonNumarasi)
        {
            if (string.IsNullOrEmpty(telefonNumarasi))
                return "";

            telefonNumarasi = telefonNumarasi.Trim();
            telefonNumarasi = telefonNumarasi.Replace("(", "");
            telefonNumarasi = telefonNumarasi.Replace(")", "");
            telefonNumarasi = telefonNumarasi.Replace("+90", "");
            telefonNumarasi = telefonNumarasi.Replace(" ", "");

            if (telefonNumarasi.Substring(0, 2) == "90")
                telefonNumarasi = telefonNumarasi.Substring(2, telefonNumarasi.Length - 2);

            if (telefonNumarasi.All(char.IsDigit) == false)
                telefonNumarasi = "";
            //if (Regex.IsMatch(telefonNumarasi, "^[0-9]*$") == false)
            //    telefonNumarasi = "";
            //foreach (char karakter in telefonNumarasi)
            //{
            //    if (!char.IsDigit(karakter))
            //    {
            //        telefonNumarasi = "";
            //        break;
            //    }
            //}

            if (telefonNumarasi.Substring(0, 1) != "5")
                telefonNumarasi = "";

            if (telefonNumarasi.Length != 10)
                telefonNumarasi = "";

            return telefonNumarasi;
            /*
            // Farklı telefon numarası formatlarını kontrol eden düzenli ifadeler
            string[] telefonNumarasiDesenleri = {
            @"^\(5\d{3}\) \d{3} \d{4}$", // (5XX) XXX XXXX
            @"^5\d{9}$",                 // 5XXXXXXXXX
            @"^\+905\d{9}$",             // +905XXXXXXXXX
            @"^05\d{9}$"                 // 05XXXXXXXXX
            };

            foreach (string desen in telefonNumarasiDesenleri)
            {
                if (Regex.IsMatch(telefonNumarasi, desen))
                {
                }
            }

            return "";
            */
        }
        
        public string SmsTarihKontrolu(string tarih)
        {
            try
            {
                tarih = tarih.Trim();
                Str_Replace(ref tarih, " ", "");
                Str_Replace(ref tarih, ".", "");
                Str_Replace(ref tarih, ":", "");
                if (tarih.Length > 8)
                    tarih = tarih.Substring(0, 8); //ddmmyyyy
                return tarih;
            }
            catch (Exception)
            {
                return "";
                //throw;
            }
        }
        
        public string SmsSaatKontrolu(string saat)
        {
            try
            {
                saat = saat.Trim();
                saat = saat.Replace(" ", "");
                saat = saat.Replace(".", "");
                saat = saat.Replace(":", "");
                if (saat.Length > 4)
                    saat = saat.Substring(0, 4);
                return saat;
            }
            catch (Exception)
            {
                return "";
                //throw;
            }
        }        
        #endregion

        #region myControl_Size_And_Location
        public void myControl_Size_And_Location(Control tControl,
                                                int width, int height, int left, int top)
        {
            if (width == 0) width = 250;
            if (height == 0) height = 250;

            if ((width > 0) || (height > 0))
                tControl.Size = new System.Drawing.Size(width, height);

            tControl.Location = new Point(left, top);
        }
        #endregion myControl_Size_And_Location

        #region myVisible

        public Boolean myVisible(string Veri)
        {
            Boolean onay;
            if (Veri.ToUpper() == "TRUE") onay = true;
            else onay = false;
            return onay;
        }

        #endregion 

        #region myBlock
        public string myBlock(string prp_type, string Kim)
        {
            string s = string.Empty;
            if (prp_type == "tPropertiesEdit")
            {
                //lockB
                if (Kim.ToUpper() == "BEGIN") s = "//<ALOCK_0>";
                //lockE
                if (Kim.ToUpper() == "END") s = "//<ALOCK_1>";
            }
            else
            {
                //lockB
                if (Kim.ToUpper() == "BEGIN") s = "//<BLOCK_0>";
                //lockE
                if (Kim.ToUpper() == "END") s = "//<BLOCK_1>";
            }

            return s;
        }
        #endregion myBlock

        #region myDouble

        public double myDouble(string Veri)
        {
            double j = 0.0;

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                    if (Veri[i] == ',')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "0.0";

            try
            {
                j = Convert.ToDouble(s);

            }
            catch (Exception)
            {
                j = 0.0;  //throw;
            }

            return j;
        }

        #endregion myDouble

        #region myInt32

        public int myInt32(string Veri)
        {
            int j = 0;

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                    if (Veri[i] == ',')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "0";

            try
            {
                j = Convert.ToInt32(s);

            }
            catch (Exception)
            {
                if (s.IndexOf(',') > -1)
                    MessageBox.Show("DİKKAT : myInt32 çevrim için gelen rakam double cinsinden !");

                j = 0;  //throw;
            }

            return j;
        }

        public long myLong(string Veri)
        {
            long j = 0;

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                    if (Veri[i] == ',')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "0";

            try
            {
                j = Convert.ToInt64(s);
            }
            catch (Exception)
            {
                if (s.IndexOf(',') > -1)
                    MessageBox.Show("DİKKAT : myLong çevrim için gelen rakam double cinsinden !");

                j = 0;  //throw;
            }

            return j;
        }

        #endregion myInt32

        #region myInt16
        public Int16 myInt16(string Veri)
        {
            Int16 j = 0;

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "0";

            try
            {
                j = Convert.ToInt16(s);
            }
            catch (Exception)
            {
                j = 0;  //throw;
            }

            return j;
        }

        #endregion myInt16

        #region myByte

        public byte myByte(string Veri)
        {
            byte j = 0;

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "0";

            try
            {
                j = Convert.ToByte(s);
            }
            catch (Exception)
            {
                j = 0;  //throw;
            }

            return j;
        }

        #endregion myByte

        #region myBool

        public bool myBool(string Veri)
        {
            bool j = false;

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "0";

            try
            {
                j = Convert.ToBoolean(s);
            }
            catch (Exception)
            {
                j = false;  //throw;
            }

            return j;
        }

        public string myBoolMsSql(string Veri)
        {
            string j = "0";

            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (char.IsDigit(Veri[i]) == true)
                {
                    s += Veri[i].ToString();
                }
                else
                {
                    if (Veri[i] == '-')
                        s += Veri[i].ToString();
                }
            }

            if (s == "") s = "False";

            try
            {
                if (Veri.ToUpper() == "TRUE") j = "1";
                if (Veri.ToUpper() == "FALSE") j = "0";
            }
            catch (Exception)
            {
                j = "0";  //throw;
            }

            return j;
        }


        #endregion myBool


        #region myTamliBolen
        public int myTamliBolen(int Bolunen, int Bolen)
        {
            int kalan = 0;
            int adet = 0;
            adet = Bolunen / Bolen;
            kalan = Bolunen % Bolen;

            if (kalan > 0) adet++;

            return adet;
        }
        #endregion

        #region myFieldValue

        public string myFieldValue(DataSet ds, string GetFieldName, string FieldName, string Value)
        {
            string j = "";

            int i2 = ds.Tables[0].Rows.Count;
            for (int i = 0; i < i2; i++)
            {
                if (ds.Tables[0].Rows[i][FieldName].ToString() == Value)
                {
                    j = ds.Tables[0].Rows[i][GetFieldName].ToString();
                    break;
                }
            }

            return j;
        }

        public string myFieldValue(DataSet ds, string GetFieldName,
               string FieldName1, string Value1, string FieldName2, string Value2)
        {
            string j = "";

            int i2 = ds.Tables[0].Rows.Count;
            for (int i = 0; i < i2; i++)
            {
                if ((ds.Tables[0].Rows[i][FieldName1].ToString() == Value1) &&
                    (ds.Tables[0].Rows[i][FieldName2].ToString() == Value2))
                {
                    j = ds.Tables[0].Rows[i][GetFieldName].ToString();
                    break;
                }
            }

            return j;
        }

        #endregion myFieldValue

        #region myFieldValueCount
        public int myFieldValueCount(DataSet ds, string FieldName, string Value)
        {
            int j = 0;

            foreach (DataRow Row in ds.Tables[0].Rows)
            {
                if (Row[FieldName].ToString() == Value) j++;
            }

            return j;
        }
        #endregion myFieldValueCount

        #region myLkpOnayCount

        public int myLkpOnayCount(DataSet ds,
                                  int MinCount, string MinUyarisi,
                                  int MaxCount, string MaxUyarisi,
                                  string MaxIslemi)
        {
            int j = 0;

            foreach (DataRow Row in ds.Tables[0].Rows)
            {
                if (Row["LKP_ONAY"].ToString() == "1") j++;
            }

            if ((MinCount > -1) && (MinCount == j))
            {
                if (IsNotNull(MinUyarisi))
                {
                    MessageBox.Show(MinUyarisi);
                }
            }

            if ((MaxCount > -1) && (MaxCount < j))
            {
                if (IsNotNull(MaxUyarisi))
                {
                    MessageBox.Show(MaxUyarisi);
                }

                if (MaxIslemi == "SIL")
                {
                    foreach (DataRow Row in ds.Tables[0].Rows)
                    {
                        if (Row["LKP_ONAY"].ToString() == "1") Row["LKP_ONAY"] = 0;
                    }
                }

            }

            return j;
        }

        #endregion myLkpOnayCount

        #region myLkpOnayPositionSet

        public void myLkpOnayPosition(DataSet ds, DataNavigator tDataNavigator)
        {
            int j = 0;
            foreach (DataRow Row in ds.Tables[0].Rows)
            {
                if (Row["LKP_ONAY"].ToString() == "1")
                {
                    tDataNavigator.Position = j;
                    tDataNavigator.Tag = j;
                    break;
                }
                j++;
            }
        }

        #endregion myLkpOnayPositionSet

        #region myMaxValue

        public int myMaxValue(string Veri)
        {
            int j = 0;
            j = myInt32(Veri);

            if (j >= v.con_Value_Max)
            {
                v.con_Value_Max = j;
            }

            return v.con_Value_Max;
        }

        #endregion myMaxValue

        #region myMinValue

        public int myMinValue(string Veri)
        {
            int j = 0;
            j = myInt32(Veri);

            if (j <= v.con_Value_Min)
            {
                v.con_Value_Min = j;
            }

            return v.con_Value_Min;
        }

        #endregion myMinValue

        #region myOperandControl

        public v.tCharacterType GetCharacterType(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return v.tCharacterType.Empty;

            // rakam kontrolü
            bool hasDigit = input.Any(char.IsDigit);
            // string kontrolü
            bool hasLetter = input.Any(char.IsLetter);

            if (hasDigit && hasLetter)
                return v.tCharacterType.Mixed; // karışık
            else if (hasDigit)
                return v.tCharacterType.OnlyDigit; // sadece rakam
            else if (hasLetter)
                return v.tCharacterType.OnlyLetter; // sadece string
            else
                return v.tCharacterType.Mixed; // Diğer karakter türleri varsa (örnek: !@#)
        }


        public bool myOperandControl(string Value1, string Value2, string OperandType)
        {
            bool onay = false;

            /* 'KRT_ODD_NOTLIKE_STR'
            1, '>=', '>=');
            2, '>', '>');
            3, '=', '=');
            4, '<=', '<=');
            5, '<', '<');
            8, '<>', '<>');
           18, 'in', 'in'
            */

            if (OperandType == ">=")
            {
                //if (myInt32(Value1) >= myInt32(Value2)) onay = true;
                if (myDouble(Value1) >= myDouble(Value2)) onay = true;
            }

            if (OperandType == ">")
            {
                //if (myInt32(Value1) > myInt32(Value2)) onay = true;
                if (myDouble(Value1) > myDouble(Value2)) onay = true;
            }

            if (OperandType == "=")
            {
                v.tCharacterType isRakam1 = GetCharacterType(Value1);
                v.tCharacterType isRakam2 = GetCharacterType(Value2);

                if (isRakam1 == v.tCharacterType.OnlyDigit &&
                    isRakam2 == v.tCharacterType.OnlyDigit)
                {
                    if (myDouble(Value1) == myDouble(Value2)) onay = true;
                }
                else
                    if (Value1 == Value2) onay = true;
            }

            if (OperandType == "<=")
            {
                //if (myInt32(Value1) <= myInt32(Value2)) onay = true;
                if (myDouble(Value1) <= myDouble(Value2)) onay = true;
            }

            if (OperandType == "<")
            {
                //if (myInt32(Value1) < myInt32(Value2)) onay = true;
                if (myDouble(Value1) < myDouble(Value2)) onay = true;
            }

            if (OperandType == "<>")
            {
                if (Value1 != Value2) onay = true;
            }

            if (OperandType == "in")
            {
                if (Value2.IndexOf(Value1) > -1) onay = true;
            }

            return onay;
        }


        #endregion myOperandControl

        #region Str_Check

        public string AntiStr_Dot(string Veri)
        {
            // string içindeki . işaareti _ işaretile değiştiriliyor
            Str_Replace(ref Veri, (char)46, (char)95);
            return Veri;
        }

        /*  (char)34 = "    (char)39 = '   */
        public string Str_Check(string Veri)
        {
            // string içindeki ' işaareti " işaretile değiştiriliyor
            Str_Replace(ref Veri, (char)39, (char)34);
            return Veri;
            /*
            string s = "";
            for (int i = 0; i < Veri.Length; ++i)
            {
                if (Veri[i].ToString() != "'")
                { s += Veri[i].ToString(); }
                else
                { s += '"'; };
            }

            return s;
            */
        }

        public string Str_AntiCheck(string Veri)
        {
            // string içindeki " işaareti ' işaretile değiştiriliyor
            Str_Replace(ref Veri, (char)34, (char)39);
            Str_Replace(ref Veri, '½', (char)39);

            return Veri;
        }

        private string Str_CheckYilAy(string Veri)
        {
            //-- :@@YILAY
            int i1 = Veri.IndexOf("-- :@@YILAY");
            if (i1 == -1)
                Veri.IndexOf("--  :@@YILAY");
            if (i1 == -1)
                i1 = Veri.IndexOf("--:@@YILAY");

            int i2 = Veri.IndexOf(":DONEM_YILAY");

            // eğer :@@YILAY ifadesi var, :DONEM_YILAY yoksa
            // :DONEM_YILAY value değerini yenileyebilmek için tekrar düzenleme gerekiyor
            //
            // declare @DonemTipiId int = 202205 -- :@@YILAY   şeklindeki satır
            // declare @DonemTipiId int = :DONEM_YILAY  -- :@@YILAY şekline dönüyor
            if ((i1 > -1) && (i2 == -1))
            {
                for (int i = i1; i > -1; i--)
                {
                    if (Veri[i] == '=' || Veri[i] == '>' || Veri[i] == '<')
                    {
                        i2 = i;
                        break;
                    }
                }

                Veri = Veri.Remove(i2 + 1, i1 - i2 - 1);

                Veri = Veri.Insert(i2 + 1, " :DONEM_YILAY ");
            }

            return Veri;
        }

        #endregion Str_Check

        #region Str_Replace ( Str Değiştirme )
        /*  (char)34 = "    (char)39 = '   */
        public string Str_Replace(ref string Veri, string mevcut_ifade, string yeni_ifade)
        {
            if (Veri.Length > 0)
                Veri = Veri.Replace(mevcut_ifade, yeni_ifade);
            return Veri;
        }

        public string Str_Replace(ref string Veri, char mevcut_ifade, char yeni_ifade)
        {
            if (Veri.Length > 0)
                Veri = Veri.Replace(mevcut_ifade, yeni_ifade);
            return Veri;
        }

        #endregion 
                
        #region Str_Remove

        public void Str_Remove(ref string Veri, string silinecek_ifade)
        {
            if (Veri.Length > 0)
            {
                int i1 = Veri.IndexOf(silinecek_ifade);
                int i2 = silinecek_ifade.Length;
                if (i1 > -1) Veri = Veri.Remove(i1, i2);
            }
        }

        #endregion Str_Remove

        #region myGetValue
        public string myGetValue(string fullText, string findText, string endText)
        {
            string value = "";
            string text = fullText;
            //findText = "ASP.NET_SessionId=";
            //endText  = ";";

            text = text.Remove(0, text.IndexOf(findText));
            int pos = text.IndexOf(endText);
            text = text.Remove(pos, text.Length - pos);
            value = text;

            return value;
        }
        #endregion myGetValue

        #region String Parcala
        public void String_Parcala(string Veri, ref string ABlock, ref string BBlock, string Bolucu)
        {
            int i = 0;

            i = Veri.IndexOf(Bolucu);
            if (i > 0)
            {
                ABlock = Veri.Substring(0, i);
                Veri = Veri.Remove(0, i + Bolucu.Length);
                BBlock = Veri;
            }
            else
            {
                ABlock = "null";
                BBlock = "null";
            }

        }
        #endregion String Parcala

        #region IsData
        public Boolean IsData(ref string Veri, string ArananData)
        {
            Boolean onay = false;

            if (IsNotNull(Veri))
            {
                //int i1 = Veri.IndexOf("=" + ArananData + ":");
                //int i2 = Veri.IndexOf("=" + ArananData + ":FIN");
                int i1 = Veri.IndexOf(ArananData + "={");   // 0=AUTO_LST:AUTO_LST={
                int i2 = Veri.IndexOf(ArananData + "=};");  // AUTO_LST=};


                // 4 nedir  ={\r\n  ArananData sonudaki işaretler
                int i3 = ArananData.Length + 4;
                int i4 = i2 - (i1 + i3);

                if (i4 > 10)
                {
                    string s = Get_And_Clear(ref Veri, ArananData + "={");
                    s = Get_And_Clear(ref Veri, ArananData + "=}"); ;
                    Veri = s.Trim();
                    onay = true;
                }
            }

            return onay;

        }
        #endregion IsData

        #region IsFile
        public bool IsFile(string pathFileName)
        {
            bool onay = false;

            FileAttributes attributes = File.GetAttributes(pathFileName);

            if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
                onay = true;

            return onay;
        }
        #endregion IsFile 

        #region IsDirectory
        public bool IsDirectory(string pathName)
        {
            bool onay = false;

            Str_Remove(ref pathName, "...");

            FileAttributes attributes = File.GetAttributes(pathName);

            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                onay = true;

            return onay;
        }
        #endregion IsDirectory

        #region UseDirectory
        public void UseDirectory(string pathName)
        {
            if (!Directory.Exists(pathName))
            {
                Directory.CreateDirectory(pathName);
            }
        }
        #endregion UseDirectory

        #region IsNull
        public string IsNull(string Veri, string Dolgu)
        {
            if ((Veri == null) || (Veri == ""))
                Veri = Dolgu;

            return Veri;
        }
        #endregion IsNull 

        #region IsNotNull
        public Boolean IsNotNull(string Veri)
        {
            if ((Veri != "") &&
                (Veri != "null") &&
                (Veri != null) &&
                (Veri != string.Empty))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean IsNotNull(DataSet dsData)
        {
            if (dsData != null)
            {
                if (dsData.Tables.Count > 0)
                {

                    if (dsData.Tables[0].Rows.Count > 0)
                    {
                        //if ((dsData.Tables[0].Rows.Count == 1) &&
                        //(dsData.Tables[0].Columns.Count == 1) &&
                        //(dsData.Tables[0].Columns[0].Caption == "@@VERSION")) return false;

                        return true;
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        #endregion IsNotNull

        #region Set
       
        public byte Set(string Value1, string Value2, byte Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToByte(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToByte(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        public Boolean Set(string Value1, string Value2, Boolean Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                if ((Value1 == "True") || (Value1 == "1"))
                    return true;
                if ((Value1 == "False") || (Value1 == "0"))
                    return false;
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                if ((Value2 == "True") || (Value2 == "1"))
                    return true;
                if ((Value2 == "False") || (Value2 == "0"))
                    return false;
            }

            return Default;
        }

        public Int16 Set(string Value1, string Value2, Int16 Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToInt16(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToInt16(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        public int Set(string Value1, string Value2, int Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToInt32(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToInt32(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        public float Set(string Value1, string Value2, float Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToSingle(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToSingle(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        public double Set(string Value1, string Value2, double Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToDouble(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToDouble(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        public decimal Set(string Value1, string Value2, decimal Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToDecimal(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToDecimal(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        public string Set(string Value1, string Value2, string Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != "") && (Value1 != "null"))
            {
                return Value1;
            }

            if (((Value1 == null) || (Value1 == "") || (Value1 == "null")) &&
                ((Value2 != null) && (Value2 != "") && (Value2 != "null")))
            {
                return Value2;
            }

            return Default;
        }

        public DateTime Set(string Value1, string Value2, DateTime Default)
        {
            // Hangi değer atanır?, Öncelik sırası nedir ?
            // 1. Value1
            // 2. Value2
            // 3. Default  atanır

            // Value1 boş değilse
            if ((Value1 != null) && (Value1 != ""))
            {
                try
                {
                    return Convert.ToDateTime(Value1);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            if (((Value1 == null) || (Value1 == "")) &&
                ((Value2 != null) && (Value2 != "")))
            {
                try
                {
                    return Convert.ToDateTime(Value2);
                }
                catch (Exception)
                {
                    return Default;
                    throw;
                }
            }

            return Default;
        }

        #endregion Set 

        #region Alias_Clear
        public string Alias_Clear(string Veri)
        {
            int i = 0;
            string fname = Veri;

            i = Veri.IndexOf(".");
            if (i > 0)
            {
                fname = Veri.Substring(i + 1, Veri.Length - (i + 1));
            }

            return fname;
        }
        #endregion

        #region TableIPCode_Get
        public void TableIPCode_Get(string TableIPCode, 
             ref string softCode, ref string projectCode, 
             ref string TableCode, ref string IPCode)
        {
            /* 
            string Table_Code = string.Empty;
            string IP_Code = string.Empty;

            tToolBox t = new tToolBox();
            t.TableIPCode_Get(Table_IP_Code, ref Table_Code, ref IP_Code);
            */

            TableIPCode = TableIPCode.Trim();

            int i = 0;
            i = TableIPCode.IndexOf(".");

            if (i > 0)
            {
                TableCode = TableIPCode.Substring(0, i);
                
                softCode = Get_And_Clear(ref TableCode, "/");
                projectCode = Get_And_Clear(ref TableCode, "/");

                TableIPCode = TableIPCode.Remove(0, i + 1);
                IPCode = TableIPCode;
            }
            else
            {
                TableCode = "null";
                IPCode = "null";
            }
        }
        #endregion TableIPCode_Get  

        #region Get_And_Clear

        public string Get_And_Clear(ref string Veri, string Separator)
        {
            string snc = ""; // Veri;
            int i1 = Veri.IndexOf(Separator);
            if (i1 > -1)
            {
                // separator karekter sayısı 
                int i2 = Separator.Length;

                // separatörden önceki veri
                snc = Veri.Substring(0, i1);

                // separatörden sonra kalan kısım
                Veri = Veri.Remove(0, i1 + i2);
            }
            return snc;
        }

        public string BeforeGet_And_AfterClear(ref string Veri, string Separator, Boolean Separator_Dahil)
        {
            string snc = Veri;
            int i1 = Veri.IndexOf(Separator);
            if (i1 > -1)
            {
                int i2 = 0;
                int i3 = Separator.Length;
                if (Separator_Dahil) i2 = i3;

                snc = Veri.Substring(0, i1 + i2);

                if (Separator_Dahil)
                    Veri = Veri.Remove(0, i1 + i2);
                else
                {
                    Veri = Veri.Remove(0, i1 + i3);
                }
            }
            return snc;
        }

        public string AfterGet_And_BeforeClear(ref string Veri, string Separator, Boolean Separator_Dahil)
        {
            string snc = Veri;
            int i1 = Veri.IndexOf(Separator);
            if (i1 > -1)
            {
                int i2 = 0;
                int i3 = Separator.Length;
                int i4 = Veri.Length;

                if (Separator_Dahil) i2 = i3;

                snc = Veri.Substring(i1 + i2, i4 - (i1 + i2));

                Veri = Veri.Remove(i1 + i2, i4 - (i1 + i2));
            }
            return snc;
        }



        #endregion Get_And_Clear

        #region Set_FieldName_Value

        public string Set_FieldName_Value(DataSet dsFields, string Field_Name, string fvalue, string SetType, string OperandType)
        {
            string MyStr = string.Empty;
            Int16 ftype = 0;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                if (Row["LKP_FIELD_NAME"].ToString() == Field_Name)
                {
                    //ftype = Set(Row["user_type_id"].ToString(), "", (Int16)167);
                    ftype = Set(Row["LKP_FIELD_TYPE"].ToString(), "", (Int16)167);
                    break;
                }
            }

            if ((ftype == 0) && (IsNotNull(Field_Name) != false))
            {
                MessageBox.Show("DİKKAT : İşlem yapmak istediğiniz field [dsFields].[" + Field_Name + "] listesinde bulunamadı.  " + v.ENTER +
                " TableIPCode - Fields listesini kontrol ediniz. Tanımlı değildir ... ", "Set_FieldName_Value");
            }

            MyStr = Set_FieldName_Value_(ftype, Field_Name, fvalue, SetType, OperandType);
            return MyStr;
        }

        public string Set_FieldName_Value_(Int16 ftype, string Field_Name, string fvalue,
                                           string SetType, string OperandType)
        {
            string MyStr = string.Empty;
            string Operand = " = ";

            // SetType  ( and ) ise   MyStr = Field_Name + " = " + fvalue;
            // SetType  ( as  ) ise   MyStr = fvalue + " as " + Field_Name;
            // SetType  ( @   ) ise   MyStr = Field_Name + " = " + fvalue;
            // SetType  ( in  ) ise   
            //
            //   MyStr =     [K].GRUP_ID in (   
            //               / *[K].GRUP_ID_INLIST* /
            //                 2990
            //               , 2983
            //               / *[K].GRUP_ID_INEND* /
            //               )
            //

            // fvalue = -2 gelirse Tumu anlamına gelmektedir

            #region toperand_type
            /* 
                   0,  '' yani =  
                   1,  'Even (Double)'
                   2,  'Odd  (Single)'
                   3,  'Speed'
                   4,  'On/Off'
                   9,  'Visible=False'
                  11,  '>='
                  12,  '>'
                  13,  '<='
                  14,  '<'
                  15,  '<>'
                  16,  'Benzerleri (%abc%)'
                  17,  'Benzerleri (abc%)'
            */
            #endregion toperand_type

            if (OperandType == "null") Operand = "";
            if (OperandType == "=") Operand = " = ";
            if (OperandType == "0") Operand = " = ";
            if ((OperandType == "11") || (OperandType == ">=")) Operand = " >= ";
            if ((OperandType == "12") || (OperandType == ">")) Operand = " > ";
            if ((OperandType == "13") || (OperandType == "<=")) Operand = " <= ";
            if ((OperandType == "14") || (OperandType == "<")) Operand = " < ";
            if ((OperandType == "15") || (OperandType == "<>")) Operand = " <> ";

            if (SetType == "@")
            {
                // @GCB_ID   >>  GCB   oluyor
                // fieldname önünde bu işaret varsa silelim
                if (Field_Name.IndexOf("@") > -1)
                    Field_Name = Field_Name.Substring(1, Field_Name.Length - 1);
            }

            //* rakam  56, 48, 127, 52, 60, 62, 59, 108
            if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 108))
            {
                // Eğer veri (Rakam) yok ise Sıfır bas
                if ((fvalue == "") ||
                    (fvalue == null) ||
                    (fvalue == "null") ||
                    (fvalue == "NewRecord")) fvalue = "-1";

                if (Field_Name == "FirmId" && fvalue == "first") fvalue = ":FIRM_ID";
                if (Field_Name == "UserId" && fvalue == "first") fvalue = ":USER_ID";

                if (fvalue == "first") fvalue = "-1";// "-99";

                // rakam türü , ile ayrılmış ise , yerine . işareti değiştiriliyor 
                if (fvalue.IndexOf(",") > -1)
                    fvalue = fvalue.Replace(",", ".");

                // alınan değeri şekillendir
                if (SetType == "and") MyStr = Field_Name + Operand + fvalue;
                if ((SetType == "and") && (fvalue == "-2")) MyStr = Field_Name + " > " + fvalue;

                if (SetType == "as") MyStr = fvalue + " as " + Field_Name;
                if (SetType == "@") MyStr = "@" + Field_Name + Operand + fvalue;
                if (SetType == "in")
                    MyStr = Field_Name + " in ( " + v.ENTER +
                        "/*" + Field_Name + "_INLIST*/" + v.ENTER +
                        fvalue + v.ENTER +
                        "/*" + Field_Name + "_INEND*/" + v.ENTER + "  )  ";
            }

            //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239, (nvarchar)231, (uniqueidentifier)36
            if ((ftype == 175) | (ftype == 167) | (ftype == 99) | (ftype == 35) | (ftype == 239) | (ftype == 231) | (ftype == 36))
            {
                // Eğer veri yok ise null bas
                if ((fvalue == "") ||
                    (fvalue == null) ||
                    (fvalue == "NewRecord")) fvalue = "";

                //if (fvalue == "null") fvalue = "null"; ??? çözüm bul

                if (fvalue == "first") fvalue = "null"; // "-99";

                if ((SetType == "and") && (fvalue != string.Empty)) MyStr = Field_Name + Operand + "'" + Str_Check(fvalue) + "' ";
                if ((SetType == "and") && (fvalue == "")) MyStr = Field_Name + Operand + " '' ";
                if ((SetType == "and") && (fvalue == "-2")) MyStr = Field_Name + " <> " + "'" + Str_Check(fvalue) + "' ";


                if (((SetType == "andlike") || (OperandType == "16")) && (fvalue != string.Empty))
                    MyStr = Field_Name + " like " + "'%" + Str_Check(fvalue) + "%' ";

                if (((SetType == "andlike") || (OperandType == "17")) && (fvalue != string.Empty))
                    MyStr = Field_Name + " like " + "'" + Str_Check(fvalue) + "%' ";

                if ((SetType == "andlike") && (fvalue == ""))
                    MyStr = Field_Name + " like '%' ";


                if ((SetType == "@") && (fvalue != string.Empty)) MyStr = "@" + Field_Name + Operand + "'" + fvalue + "' ";
                if ((SetType == "@") && (fvalue == "")) MyStr = "@" + Field_Name + Operand + " '' ";

                if (SetType == "as") 
                    MyStr = "'" + fvalue + "'" + " as " + Field_Name;

            }

            //* bit türü 104
            if (ftype == 104)
            {
                if ((fvalue == "") ||
                    (fvalue == null) ||
                    (fvalue == "null") ||
                    (fvalue == "NewRecord") ||
                    (fvalue == "0") ||
                    (fvalue == "False"))
                {
                    if (SetType == "and") MyStr = Field_Name + Operand + " 0 ";
                    if (SetType == "as") MyStr = " 0 " + " as " + Field_Name;
                    if (SetType == "@") MyStr = "@" + Field_Name + Operand + " 0 ";
                }
                else
                {
                    if (SetType == "and") MyStr = Field_Name + Operand + " 1 ";
                    if ((SetType == "and") && (fvalue == "-2")) MyStr = Field_Name + " > " + " -2 ";

                    if (SetType == "as") MyStr = " 1 " + " as " + Field_Name;
                    if (SetType == "@") MyStr = "@" + Field_Name + Operand + " 1 ";
                }
            }

            //* smalldatetime = 58
            //* date = 40, 61
            //* time = 41
            if (ftype == 58)
            {
                if ((fvalue == "") || (fvalue == "-1") || (fvalue == "0"))
                    fvalue = DateTime.Now.Date.ToShortDateString();

                if ((fvalue == null) ||
                    (fvalue == "null") ||
                    (fvalue == "NewRecord") ||
                    (fvalue == "0"))
                {
                    MyStr = string.Empty;
                }
                else
                {
                    if ((fvalue != "first") && (fvalue != "-1"))
                    {
                        if (SetType == "and")
                            MyStr = "Convert(SmallDatetime, " + Field_Name + ", 101) " + Operand + TarihSaat_Formati(Convert.ToDateTime(fvalue)) + " ";

                        if (SetType == "as") MyStr = TarihSaat_Formati(Convert.ToDateTime(fvalue)) + " as " + Field_Name;
                        if (SetType == "@") MyStr = "@" + Field_Name + Operand + Tarih_Formati(Convert.ToDateTime(fvalue)) + " ";
                    }
                    if ((fvalue == "first") || (fvalue == "-99"))
                    {
                        //if (SetType == "and") MyStr = "Convert(Datetime, Convert(varchar(10)," + Field_Name + ", 103), 103) " + Operand + "Convert(Datetime,'01.01.1900', 103) ";

                        if ((v.active_DB.projectDBType == v.dBaseType.MSSQL) && (SetType == "and"))
                            MyStr = "Convert(Datetime, " + Field_Name + ", 103) " + Operand + "Convert(Datetime,'01.01.1900', 103) ";

                        if (SetType == "as") MyStr = "'01.01.1900'" + " as " + Field_Name;
                        if (SetType == "@") MyStr = "@" + Field_Name + Operand + "'01.01.1900'" + " ";
                    }
                }
            }
            //* date = 40, 61
            //* time = 41
            if ((ftype == 40) || (ftype == 61))
            {
                //if ((fvalue == "") || (fvalue == "-1") || (fvalue == "0"))
                //    fvalue = DateTime.Now.Date.ToShortDateString();

                if ((fvalue == null) ||
                    (fvalue == "null") ||
                    (fvalue == "NewRecord") ||
                    (fvalue == "0") ||
                    (fvalue == "") /// yeni eklendi
                    )
                {
                    if (SetType != "@")
                    {
                        //MyStr = string.Empty;
                        MyStr = "Convert(Date, isnull(" + Field_Name + ",getdate()), 103) " + " <> " + " Convert(Date, '01.01.1900', 103)";
                    }
                    else
                    {
                        // /*prm*/ set @FirmId = 0   -- :D.SD.64295: --
                        // /*prm*/ set @BaslangicTarihi = Convert(Date, '01.09.2025', 103)-- :D.SD.64297: --

                        MyStr = "@" + Field_Name + " = " + " Convert(Date, '01.01.1900', 103)  -- :D.SD.";
                    }
                }
                else
                {
                    if ((fvalue != "first") && (fvalue != "-1"))
                    {
                        if ((v.active_DB.projectDBType == v.dBaseType.MSSQL) && (SetType == "and"))
                            MyStr = "Convert(Date, " + Field_Name + ", 103) " + Operand + Tarih_Formati(Convert.ToDateTime(fvalue)) + " ";

                        if (SetType == "as") MyStr = Tarih_Formati(Convert.ToDateTime(fvalue)) + " as " + Field_Name;
                        if (SetType == "@") MyStr = "@" + Field_Name + Operand + Tarih_Formati(Convert.ToDateTime(fvalue)) + " ";
                    }
                    if ((fvalue == "first") || (fvalue == "-99"))
                    {
                        if ((v.active_DB.
                            projectDBType == v.dBaseType.MSSQL) && (SetType == "and"))
                            MyStr = "Convert(Date, " + Field_Name + ", 103) " + Operand + "Convert(Date,'01.01.1900', 103) ";

                        if (SetType == "as") MyStr = "'01.01.1900'" + " as " + Field_Name;
                        if (SetType == "@") MyStr = "@" + Field_Name + Operand + "'01.01.1900'" + " ";
                    }
                }
            }

            return MyStr;
        }

        #endregion Set_FieldName_Value

        #region Alias Control
        public void Alias_Control(ref string alias)
        {
            if (alias.IndexOf(".") == -1)
                alias = alias + ".";
        }
        #endregion Alias Control

        #region MySQL_Clear
        public string kisitlamalarClear(string tSQL)
        {
            if (tSQL.IndexOf("/*KISITLAMALAR|1|*/") > -1)
            {
                int i1 = tSQL.IndexOf("/*KISITLAMALAR|1|*/");
                int i2 = (tSQL.IndexOf("/*KISITLAMALAR|2|*/") - i1) + 22;
                tSQL = tSQL.Remove(i1, i2);
            }
            return tSQL;
        }
        #endregion MySQL_Clear

        #region Get_ValueTrue

        public bool Get_ValueTrue(int ftype, string read_value)
        {
            bool onay = false;

            //* rakam  56, 48, 127, 52, 60, 62, 59
            if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                (ftype == 60) | (ftype == 62) | (ftype == 59))
            {
                int value = myInt32(read_value);

                if (value == 0) return false;
                if (value > 0) return true;
            }

            //* numeric  108
            if ((ftype == 108))
            {
                double value = myDouble(read_value);

                if (value == 0) return false;
                if (value > 0) return true;
            }

            //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239, (nvarchar)231
            if ((ftype == 175) | (ftype == 167) | (ftype == 99) | (ftype == 35) | (ftype == 239) | (ftype == 231))
            {
                // Eğer veri yok ise null bas
                return IsNotNull(read_value);
            }

            //* bit türü 104
            if (ftype == 104)
            {
                if ((read_value == "") ||
                    (read_value == "0") ||
                    (read_value == "False")) return false;

                if ((read_value == "1") ||
                    (read_value == "True")) return true;
            }

            //* date 58
            if ((ftype == 58) || (ftype == 40) || (ftype == 61))
            {
                return IsNotNull(read_value);
            }

            return onay;
        }

        #endregion Get_ValueTrue

        #region Get_FieldTypeName

        private string getFieldTypeName(int fType)
        {
            string s = "";
            if (fType == 56) s = "Int";
            if (fType == 52) s = "SmallInt";
            if (fType == 127) s = "Bigint";
            if (fType == 104) s = "Bit";
            if (fType == 108) s = "Numeric";
            if (fType == 62) s = "Float";
            if (fType == 59) s = "Real";
            if (fType == 60) s = "Money";
            if (fType == 48) s = "Tinyint";
            if (fType == 58) s = "Smalldatetime";
            if (fType == 61) s = "Datetime";
            if (fType == 40) s = "Date";
            if (fType == 41) s = "Time";
            if (fType == 167) s = "Varchar";
            if (fType == 231) s = "nVarchar";
            if (fType == 175) s = "Char";
            if (fType == 35) s = "Text";
            if (fType == 99) s = "Ntext";
            if (fType == 34) s = "Image";
            if (fType == 173) s = "Binary";
            if (fType == 165) s = "Varbinary";
            return s;
        }

        public string Get_FieldTypeName(int ftype, string value)
        {
            string s = string.Empty;
                        
            if ((value == "") ||
                (value == "0")) value = "100";

            if (ftype == 56) s = "int";
            if (ftype == 52) s = "smallint";
            if (ftype == 127) s = "bigint";
            if (ftype == 104) s = "bit";
            if (ftype == 108) s = "numeric(" + value + ")";
            if (ftype == 62) s = "float";
            if (ftype == 59) s = "real";
            if (ftype == 60) s = "money";
            if (ftype == 48) s = "tinyint";

            if (ftype == 231) s = "varchar(" + value + ")";
            if (ftype == 167) s = "varchar(" + value + ")";
            if (ftype == 175) s = "char(" + value + ")";
            if (ftype == 35) s = "text";
            // 36 = uniqueidentifier
            if (ftype == 36) s = "varchar(" + value + ")"; 
            if (ftype == 99) s = "ntext";

            if (ftype == 58) s = "smalldatetime";
            if ((ftype == 40) | (ftype == 61)) s = "date";

            if (ftype == 41) s = "time";

            if (ftype == 34) s = "image";
            if (ftype == 173) s = "binary";
            if (ftype == 165) s = "varbinary";

            return s;
        }

        #endregion Get_FieldTypeName

        #region tField_Convert
        public string tField_Convert(string fname, int ftype)
        {
            // işleme uğramaz ise geldiği gibi dönsün
            string sonuc = fname;

            //* date türü 40
            if ((ftype == 40) || (ftype == 61))
            {
                if (fname != "")
                {
                    if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                        sonuc = " Convert(Datetime, Convert(varchar(10)," + fname + ", 103), 104) ";  //  Tarih_Formati(Convert.ToDateTime(fvalue)) + " ";
                }
            }

            return sonuc;
        }
        #endregion tField_Convert

        #region tData_Convert
        public string tData_Convert(string fvalue, int ftype)
        {
            string sonuc = "";

            // eğer tanımsız ise default veri tipi VarChar olsun
            if (ftype == 0) ftype = 167;

            //* rakam  56, 48, 127, 52, 60, 62, 59, 108
            if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 108))
            {
                // Eğer veri (Rakam) yok ise Sıfır bas
                //if (fvalue == "") fvalue = "0";

                // rakam türü , ile ayrılmış ise , yerine . işareti değiştiriliyor 
                if (fvalue.IndexOf(",") > -1)
                    fvalue = fvalue.Replace(",", ".");

                sonuc = fvalue;
            }

            //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239
            if ((ftype == 175) | (ftype == 167) | (ftype == 99) | (ftype == 35) | (ftype == 239))
            {
                // Eğer veri yok ise null bas
                if ((fvalue == "") | (fvalue == "null"))
                    sonuc = "null";
                else
                    sonuc = " '" + Str_Check(fvalue) + "' ";
            }

            //* bit türü 104
            if (ftype == 104)
            {
                if ((fvalue == "") | (fvalue == "0") | (fvalue == "False"))
                    sonuc = " 0 ";
                else sonuc = " 1 ";
            }

            //* datetime 58
            if ((ftype == 58) || (ftype == 581))
            {
                if (fvalue != "")
                    sonuc = " " + TarihSaat_Formati(Convert.ToDateTime(fvalue)) + " ";
                else sonuc = "";
            }

            if (ftype == 582)
            {
                if (fvalue != "")
                    sonuc = " " + TarihSaat_Formati_End(Convert.ToDateTime(fvalue)) + " ";
                else sonuc = "";
            }


            //* date türü 40
            if ((ftype == 40) || (ftype == 61))
            {
                if (fvalue != "")
                    sonuc = " " + Tarih_Formati(Convert.ToDateTime(fvalue)) + " ";
            }


            // new CheckedComboBoxEdit
            #region
            if ((ftype == 1672) || (ftype == 562))
            {
                string value = string.Empty;
                sonuc = string.Empty;
                int p1 = 0;
                int p2 = 0;

                // köşeli parentez içindeki değerler alıncak ve 
                // sonuçta bu değerler in için hazır hale getirilecek                 

                // fvalue nin gelişi

                // [SH] Saat Hesabı, [HI] Hafta İçi, [VS] Sabah Vardiyası
                //   +  Saat Hesabı, [HI] Hafta İçi, [VS] Sabah Vardiyası
                //   +  Saat Hesabı,   +  Hafta İçi, [VS] Sabah Vardiyası
                //   +  Saat Hesabı,   +  Hafta İçi,   +  Sabah Vardiyası

                // sonuc = ('SH','HI','VS')

                while (fvalue.IndexOf("[") > -1)
                {
                    value = string.Empty;
                    p1 = fvalue.IndexOf("[");
                    p2 = fvalue.IndexOf("]");

                    if ((p1 > -1) && (p2 > -1))
                    {
                        // valueyi al
                        value = fvalue.Substring(p1 + 1, (p2 - p1) - 1);
                        // valueyi listeden çıkar
                        Str_Replace(ref fvalue, "[" + value + "]", "+");

                        // string field ise
                        if (ftype == 1672) value = "'" + value + "'";
                        // rakam field ise
                        //if (ftype == 562)

                        if (sonuc.Length == 0) sonuc = value;
                        else sonuc = sonuc + "," + value;
                    }
                }

                if (IsNotNull(sonuc))
                    sonuc = "(" + sonuc + ")";
            }
            #endregion 

            return sonuc;
        }
        #endregion tData_Convert

        #region mypropChanged
        public void mypropChanged(ref DataSet dsData, string oldValue, string newValue)
        {
            if (dsData.Namespace != null)
            {
                string myprop = dsData.Namespace.ToString();
                myprop = myprop.Replace(oldValue, newValue);
                dsData.Namespace = myprop;
                //v.Kullaniciya_Mesaj_Var = "DataState : Update, Column Changed";
            }
        }
        #endregion

        #region tSqlSecond_Set
        public void tSqlSecond_Set(ref DataSet dsData, string NewSql)
        {
            if (dsData != null)
                if (dsData.Namespace != null)
                {
                    string myProp = dsData.Namespace.ToString();
                    if (IsNotNull(myProp))
                    {
                        string OldSql = MyProperties_Get(myProp, "=SqlSecond:");

                        if (OldSql != NewSql)
                        {
                            if (OldSql == "")
                                OldSql = "=SqlSecond:null;";
                            else OldSql = "=SqlSecond:" + OldSql + ";";

                            NewSql = "=SqlSecond:" + NewSql + ";";

                            Str_Replace(ref myProp, OldSql, NewSql);
                            dsData.Namespace = myProp;
                        }
                    }
                }
        }
        #endregion tSqlSecond_Set

        #region tSqlSecond_Clear
        public void tSqlSecond_Clear(ref DataSet dsData)
        {
            if (dsData != null)
                if (dsData.Namespace != null)
                {
                    string myProp = dsData.Namespace.ToString();
                    if (IsNotNull(myProp))
                    {
                        string OldSql = MyProperties_Get(myProp, "=SqlSecond:");

                        if (OldSql != "")
                        {
                            OldSql = "=SqlSecond:" + OldSql + ";";

                            Str_Replace(ref myProp, OldSql, "=SqlSecond:null;" + v.ENTER);
                            dsData.Namespace = myProp;
                        }
                    }
                }
        }
        #endregion tSqlSecond_Set

        #region tSqlParam_Replace
        public void tSqlParam_Replace(ref string Sql, string Param, string Value)
        {
            string ParamSatiri = Param + " = ";
            int i1 = Sql.IndexOf(ParamSatiri);
            int i2 = 0;

            if (i1 > -1)
            {
                for (int i = i1; i < Sql.Length; i++)
                {
                    if (Sql[i] == '\r') { i2 = i; break; }
                }

                if ((i1 > 0) && (i2 > 0))
                {
                    string OldValue = Sql.Substring(i1, (i2 - i1));
                    string NewValue = Param + " = " + Value + "  ";
                    Str_Replace(ref Sql, OldValue, NewValue);
                }
            }
        }
        #endregion tSqlParam_Replace

        #region Kriter_Ekle

        public void tKriter_Clear(ref string aSQL)
        {
            string kriterBegin = "/*KRITERLER*/";
            string kriterEnd = "/*KRITERLER_END*/";

            int i1 = aSQL.IndexOf(kriterBegin);
            int i2 = aSQL.IndexOf(kriterEnd);

            if (i1 == -1)
            {
                MessageBox.Show("DİKKAT : SQL içinde /*KRITERLER*/ başlangıç etiketi bulunamadı.  ");
                return;
            }

            //if ((i1 > -1) && (i2 > -1))
            while (i1 > -1) 
            {
                int start = i1 + kriterBegin.Length + 1;
                int uzunluk = i2 - start;
                aSQL = aSQL.Remove(start, uzunluk);

                i2 = aSQL.IndexOf(kriterEnd, start); // silinme işleminden dolayı konumlar değişti, tekrar bulmak gerekiyor
                i1 = aSQL.IndexOf(kriterBegin, i2 + 1);
                i2 = aSQL.IndexOf(kriterEnd, i1 + 1);
            }

        }
        public void tKriter_Add(ref string aSQL, string Where_Add)
        {
            string kriterBegin = "/*KRITERLER*/";
            string kriterEnd = "/*KRITERLER_END*/";

            int i1 = aSQL.IndexOf(kriterBegin); 
            int i2 = aSQL.IndexOf(kriterEnd);

            //if (i1 > -1)
            while (i1 > -1)
            {
                i1 = i1 + kriterBegin.Length;
                aSQL = aSQL.Insert(i1, Where_Add);

                i2 = aSQL.IndexOf(kriterEnd, i1);
                i1 = aSQL.IndexOf(kriterBegin, i2 + 1);
            }
        }

        public void tKriter_Ekle(ref string aSQL, string alias, string Where_Add)
        {
            // and [VRSNL_10].{bugun} =   CONVERT(SMALLDATETIME, '06.08.2015 00:00:00', 101)  

            /*[VRSNL_10].KRITERLER*/

            if (alias.IndexOf(".") == -1) alias = alias + ".";

            int tboy = 15 + alias.Length;

            int h = 0;
            int n = 0;
            string SQLTemp = aSQL;
            int fKRITERLER = SQLTemp.IndexOf("/*" + alias + "KRITERLER*/");

            while (fKRITERLER > 0)
            {
                n = n + fKRITERLER + tboy;
                if ((fKRITERLER > tboy) && (Where_Add != ""))
                {
                    aSQL = aSQL.Insert(n, Where_Add);
                }

                h = fKRITERLER + tboy;
                n = n + Where_Add.Length;

                SQLTemp = SQLTemp.Remove(0, h);

                // birden fazla aynı alias olabilir, UNION ALL da gerekiyor

                fKRITERLER = SQLTemp.IndexOf("/*" + alias + "KRITERLER*/");
            }
        }
        #endregion Kriter_Ekle

        #region tLast_Char_Remove / En Sondaki ', ' sil
        public string tLast_Char_Remove(string Veri)
        {
            // En Sondaki ', ' siliniyor
            Veri = Veri.Trim();
            if (Veri.Length < 2)
                return Veri;
            Veri = Veri.Substring(0, Veri.Length - 1);
            return Veri;
        }
        #endregion

        #region tFindWordCount : kelime sayısını bul
        public int tFindWordCount(string text, string aranacakIfade)
        {
            string sekil = text.Replace(aranacakIfade, "*");
            //burada ben bir char istediği için * yapılmış
            int adet = sekil.Count(x => x == '*'); // kaç adet bir var saydırıyoruz.
            return adet;
        }
        #endregion tFindWordCount
        #endregion String İşlemler

        #region *MyProperties

        public void MyProperties_Set(ref string tProperties, ref string tProCaption, string object_name, string Caption, string Value)
        {
            if (Value == string.Empty) Value = "null";

            if ((Value != "BEGIN") && (Value != "END"))
            {
                tProperties = tProperties + object_name + "=" + Caption + ":" + Value + ";" + v.ENTER;
                tProCaption = tProCaption + object_name + "=" + Caption + ":";
            }

            if ((Value == "BEGIN") || (Value == "END"))
            {
                tProperties = tProperties + object_name + "=" + Value + ":" + v.ENTER;
            }
        }

        public void MyProperties_Set(ref string tProperties, string object_name, string Caption, string Value)
        {
            if (Value == string.Empty) Value = "null";

            if ((Value != "BEGIN") && (Value != "END"))
            {
                tProperties = tProperties + object_name + "=" + Caption + ":" + Value + ";" + v.ENTER;
            }
            if ((Value == "BEGIN") || (Value == "END"))
            {
                tProperties = tProperties + object_name + "=" + Value + ":" + object_name + ";" + v.ENTER;
            }
        }

        public void MyProperties_Set(ref string tProperties, string Caption, string Value)
        {
            if (Value == string.Empty) Value = "null";

            if ((Value != "BEGIN") && (Value != "END"))
            {
                tProperties = tProperties + "=" + Caption + ":" + Value + ";" + v.ENTER;
            }
            if ((Value == "BEGIN") || (Value == "END"))
            {
                tProperties = tProperties + "=" + Value + ":" + v.ENTER;
            }
        }

        public string MyProperties_Get(string tProperties, string Caption)
        {
            string MyValue = string.Empty;
            if (IsNotNull(tProperties) == false) return MyValue;
            int i1 = 0;

            i1 = tProperties.IndexOf(Caption);
            if (i1 > -1)
            {
                i1 = i1 + Caption.Length;
                MyValue = tProperties.Remove(0, i1);
                i1 = MyValue.IndexOf(";");
                if (i1 > -1)
                    MyValue = MyValue.Substring(0, i1);
                else
                {
                    // yoksa " çift tırnak ara
                    i1 = MyValue.IndexOf((char)34);
                    if (i1 > -1)
                        MyValue = MyValue.Substring(0, i1);
                }
            }

            if (MyValue == "null") MyValue = string.Empty;

            return MyValue;

            /*
                *   tBarButtonItem_84=Caption:button 3;
                *   tBarButtonItem_84=TableIPCode:22;
                *   tBarButtonItem_84=MasterTableIPCode1:TABLE_1.t11;
                *   tBarButtonItem_84=MasterFieldName11:F1;
                *   tBarButtonItem_84=MasterFieldName12:F2;
                *   tBarButtonItem_84=ParentTableIPCode1:TABLE_1.t11;
                *   tBarButtonItem_84=ParentFieldName11:F3;
                *   tBarButtonItem_84=ParentFieldName12:F4;
                *   tBarButtonItem_84=ParentTableIPCode2:TABLE_1.t11;
                *   tBarButtonItem_84=ParentFieldName21:F5;
                *   tBarButtonItem_84=ParentFieldName22:F6;
                *   tBarButtonItem_84=END:
                * 
                */
        }

        #endregion MyProperties

        #region *Get Functions 

        public v.tButtonType getClickType(string myProp)
        {
            //tToolBox t = new tToolBox();
            v.tButtonType propButtonType = v.tButtonType.btNone;
            PROP_NAVIGATOR prop_ = null;
            List<PROP_NAVIGATOR> propList_ = null;

            readProNavigator(myProp, ref prop_, ref propList_);

            if (propList_ != null)
            {
                foreach (PROP_NAVIGATOR item in propList_)
                {
                    // ilk bulduğu type gönderir
                    if (item.BUTTONTYPE.ToString() != "null")
                    {
                        return propButtonType = getClickType(Convert.ToInt32(item.BUTTONTYPE.ToString()));
                    }
                }
            }

            return propButtonType;
        }
        public v.tButtonType getClickType(int value)
        {
            if (value == 0) return v.tButtonType.btNone;
            if (value == 1) return v.tButtonType.btEnter;
            if (value == 2) return v.tButtonType.btEscape;
            if (value == 11) return v.tButtonType.btCikis;
            if (value == 999) return v.tButtonType.btCikis;

            if (value == 68) return v.tButtonType.btSihirbazDevam;
            if (value == 69) return v.tButtonType.btSihirbazGeri;

            if (value == 12) return v.tButtonType.btSecCik;
            if (value == 13) return v.tButtonType.btListeyeEkle;
            if (value == 14) return v.tButtonType.btListeHazirla;
            if (value == 15) return v.tButtonType.btListele;

            if (value == 21) return v.tButtonType.btKaydetYeni;
            if (value == 22) return v.tButtonType.btKaydet;
            if (value == 23) return v.tButtonType.btKaydetDevam;
            if (value == 24) return v.tButtonType.btKaydetCik;

            if (value == 31) return v.tButtonType.btYeniKart;
            if (value == 32) return v.tButtonType.btYeniHesap;
            if (value == 33) return v.tButtonType.btYeniBelge;
            if (value == 34) return v.tButtonType.btYeniAltHesap;

            if (value == 36) return v.tButtonType.btYeniKartSatir;
            if (value == 37) return v.tButtonType.btYeniHesapSatir;
            if (value == 38) return v.tButtonType.btYeniBelgeSatir;
            if (value == 39) return v.tButtonType.btYeniAltHesapSatir;

            if (value == 41) return v.tButtonType.btGoster;
            if (value == 42) return v.tButtonType.btKartAc;
            if (value == 43) return v.tButtonType.btHesapAc;
            if (value == 44) return v.tButtonType.btBelgeAc;
            if (value == 45) return v.tButtonType.btResimEditor;
            if (value == 46) return v.tButtonType.btReportDesign;

            // unutma
            if (value == 51) return v.tButtonType.btSilSatir;
            if (value == 52) return v.tButtonType.btSilKart;
            if (value == 53) return v.tButtonType.btSilHesap;
            if (value == 54) return v.tButtonType.btSilBelge;
            if (value == 55) return v.tButtonType.btSilListe;

            if (value == 61) return v.tButtonType.btEnSona;
            if (value == 62) return v.tButtonType.btSonrakiSayfa;
            if (value == 63) return v.tButtonType.btSonraki;
            if (value == 65) return v.tButtonType.btOnceki;
            if (value == 66) return v.tButtonType.btOncekiSayfa;
            if (value == 67) return v.tButtonType.btEnBasa;

            if (value == 71) return v.tButtonType.btCollapse;
            if (value == 72) return v.tButtonType.btExpanded;

            if (value == 73) return v.tButtonType.btOnayEkle;
            if (value == 74) return v.tButtonType.btOnayKaldir;

            if (value == 75) return v.tButtonType.btMesajGonder;

            if (value == 81) return v.tButtonType.btYazici;
            if (value == 82) return v.tButtonType.btRapor;

            if (value == 91) return v.tButtonType.btEk1;
            if (value == 92) return v.tButtonType.btEk2;
            if (value == 93) return v.tButtonType.btEk3;
            if (value == 94) return v.tButtonType.btEk4;
            if (value == 95) return v.tButtonType.btEk5;
            if (value == 96) return v.tButtonType.btEk6;
            if (value == 97) return v.tButtonType.btEk7;

            if (value == 121) return v.tButtonType.btArama;
            if (value == 122) return v.tButtonType.btFormulleriHesapla;
            if (value == 123) return v.tButtonType.btDataTransferi;
            if (value == 124) return v.tButtonType.btInputBox;
            if (value == 125) return v.tButtonType.btOpenSubView;
            if (value == 126) return v.tButtonType.btExtraIslem;
            if (value == 127) return v.tButtonType.btFindListData;

            return v.tButtonType.btNone;
        }
        public int getDataNavigatorPosition(DataNavigator dN, bool showMessage)
        {
            int pos = dN.Position;
            if (pos == -1)
            {
                try
                {
                    pos = Convert.ToInt32(dN.Tag.ToString());
                }
                catch (Exception)
                {
                    pos = -1;
                    if (showMessage)
                        MessageBox.Show("Dikkat : Data position okunurken beklenmedik bir sorun oluştu...");
                }
            }
            return pos;
        }

        public string getContent(string masterStr, string startStr, string endStr, ref int position)
        {
            // start case /*LastIdControlSql*/
            // end   case /*LastIdControlSqlEnd*/ 

            string content = "";

            int i1 = masterStr.IndexOf(startStr, position);
            if (i1 == -1) return content;

            i1 = i1 + startStr.Length + 1;

            int i2 = masterStr.IndexOf(endStr, i1);

            if (i2 > i1)
            {
                content = masterStr.Substring(i1, i2 - i1);
                position = i2 + endStr.Length;
            }

            return content;
        }

        public string getTarih(string oldValue)
        {
            vUserInputBox iBox = new vUserInputBox();

            iBox.Clear();
            iBox.title = "Tarih";
            iBox.promptText = "Tarih girin  :";
            iBox.value = oldValue;
            iBox.displayFormat = "";
            iBox.fieldType = 40; // tarih : 40 veya 61

            string userDate = "";

            if (UserInpuBox(iBox) == DialogResult.OK)
            {
                userDate = iBox.value;
            }

            if (userDate == "") userDate = "01.01.1990";

            return userDate;
        }

        public DataRow getDataRow(DataSet ds_Fields, string fieldName)
        {
            DataRow row = null;
            for (int i = 0; i < ds_Fields.Tables[0].Rows.Count; i++)
            {
                if (ds_Fields.Tables[0].Rows[i]["LKP_FIELD_NAME"].ToString() == fieldName)
                {
                    row = ds_Fields.Tables[0].Rows[i];
                    break;
                }
            }
            return row;
        }

        #region <myStart> Get
        public string myStart_Get(Form tForm)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, "tMyFormBox", "", controls);
            string s = string.Empty;

            if (c != null)
            {
                if (((DevExpress.XtraEditors.MemoEdit)c).EditValue != null)
                {
                    s = ((DevExpress.XtraEditors.MemoEdit)c).EditValue.ToString();

                    int i1 = s.IndexOf("//<myStart>");
                    int i2 = s.LastIndexOf("//<myEnd>");

                    if ((i1 > -1) && (i2 > -1))
                        s = s.Substring(i1, i2 - i1);
                }
            }

            return s;
        }
        #endregion <myStart> Get

        #region myFormBox_Values //  myFormListBox_Values
        public string myFormBox_Values(Form tForm, string ButtonName)
        {
            string s = string.Empty;
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, "tMyFormBox", "", controls);

            // ButtonName boş ise tMyFormBox üzerindeki tüm bilgiler gönderilecek
            //            boş değilse sadece ButtonName ait bilgiler gönderilecek

            if (c != null)
            {
                if (((DevExpress.XtraEditors.MemoEdit)c).EditValue != null)
                {
                    s = ((DevExpress.XtraEditors.MemoEdit)c).EditValue.ToString();
                    if (s == "[]") s = "";

                    int i1 = 0;
                    int i2 = 0;
                    if ((ButtonName != string.Empty) || (ButtonName != ""))
                    {
                        i1 = s.IndexOf(ButtonName);
                        i2 = s.LastIndexOf(ButtonName + "=END:");

                        if ((i1 > -1) && (i2 > -1))
                            s = s.Substring(i1, i2 - i1);
                    }
                }
            }

            return s;
        }
        #endregion myFormBox_Values

        #region Find_Button_Value
        public string Find_Button_Value(Form tForm, string ButtonName, string FindCode)
        {
            // 1. formun üzerinde create sırasında atanmış olan button hakkındaki bilgiler okunur
            string snc = myFormBox_Values(tForm, ButtonName);

            // 2. butona ait bilgiler ayıklanır
            return MyProperties_Get(snc, FindCode);
        }
        #endregion Find_Button_Value

        #region Find_Control_List

        public void tRemoveTabPagesForNewData(Form tForm)
        {
            
            List<string> list = new List<string>();
            FindName_Control_List(tForm, list, "tTabPage_");

            Control cntrl = null;
            foreach (string value in list)
            {
                cntrl = Find_Control(tForm, value);
                if (cntrl != null)
                    cntrl.Dispose();
            }
        }

        public void FindName_Control_List(Form tForm, List<string> list, string controlName)
        {
            list.Clear();

            foreach (Control c in tForm.Controls)
            {
                FindName_Control_List_(c, list, controlName);
            }

            list.Sort();
        }

        public void Find_Control_List(Form tForm, List<string> list, string[] ControlType, string tabPageName)
        {
            if (tForm == null) return;
            // 0 : tabpege arama yok 
            // 1 : tabpege aramaya başla
            // 2 : aranan tabPage bulundu
            // 3 : aranan tabPage işlemleri bitti 
            Int16 tabPageStatus = 0;
            if (tabPageName != "")  
                tabPageStatus = 1;

            Int16 cntrlLayer = 0;
            Int16 tabPageLayer = 0;

            list.Clear();

            foreach (Control c in tForm.Controls)
            {
                Find_Control_List_(c, list, ControlType, ref tabPageStatus, tabPageName, ref cntrlLayer, ref tabPageLayer);
            }

            list.Sort();
        }

        public void Find_Controls_List(Form tForm, string controlName, ref List<Control> list)
        {
            list.Clear();

            foreach (Control c in tForm.Controls)
            {
                Find_Controls_List_(c, controlName, list);
            }
        }
        private void Find_Controls_List_(Control cntrl, string controlName, List<Control> list)
        {
            string cName = cntrl?.Name;
            cName = cntrl?.GetType().ToString();

            if (cName?.IndexOf(controlName) > -1)
                list.Add(cntrl);

            if (cntrl.Controls.Count > 0)
            {
                foreach (Control item in cntrl.Controls)
                {
                    Find_Controls_List_(item, controlName, list);
                }
            }
        }



        private void FindName_Control_List_(Control cntrl, List<string> list, string controlName)
        {
            string cName = cntrl?.Name;
            
            if (cName?.IndexOf(controlName) > -1)
                list.Add(cntrl.Name.ToString());

            if (cntrl.Controls.Count > 0)
            {
                foreach (Control item in cntrl.Controls)
                {
                    FindName_Control_List_(item, list, controlName);
                }
            }
        }

        public void Find_Control_List(Form tForm, List<string> list, string[] ControlType)
        {
            // CheckedListBoxControl
            #region Control1
            //string dataLayout = "DevExpress.XtraDataLayout.DataLayoutControl";

            list.Clear();
            
            Int16 tabPageStatus = 0;
            Int16 cntrlLayer = 0;
            Int16 tabPageLayer = 0;

            foreach (Control c in tForm.Controls)
            {
                Find_Control_List_(c, list, ControlType, ref tabPageStatus, "", ref cntrlLayer, ref tabPageLayer);
            }

            #region old code
            /*
            foreach (Control c in tForm.Controls)
            {
                tList_Add(c, list, ControlType);

                if (c.ToString() == dataLayout)
                    tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c, list, ControlType);

                if (c.Controls.Count > 0)
                {
                    #region Control2
                    foreach (Control c2 in c.Controls)
                    {
                        tList_Add(c2, list, ControlType);

                        if (c2.ToString() == dataLayout)
                            tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c2, list, ControlType);

                        #region Control3
                        foreach (Control c3 in c2.Controls)
                        {
                            tList_Add(c3, list, ControlType);

                            if (c3.ToString() == dataLayout)
                                tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c3, list, ControlType);

                            if (c3.Controls.Count > 0)
                            {
                                #region Control4
                                foreach (Control c4 in c3.Controls)
                                {
                                    tList_Add(c4, list, ControlType);

                                    if (c4.ToString() == dataLayout)
                                        tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c4, list, ControlType);

                                    if (c4.Controls.Count > 0)
                                    {
                                        #region Control5
                                        foreach (Control c5 in c4.Controls)
                                        {
                                            tList_Add(c5, list, ControlType);

                                            if (c5.ToString() == dataLayout)
                                                tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c5, list, ControlType);

                                            if (c5.Controls.Count > 0)
                                            {
                                                #region Control6
                                                foreach (Control c6 in c5.Controls)
                                                {
                                                    tList_Add(c6, list, ControlType);

                                                    if (c6.ToString() == dataLayout)
                                                        tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c6, list, ControlType);

                                                    if (c6.Controls.Count > 0)
                                                    {
                                                        #region Control7
                                                        foreach (Control c7 in c6.Controls)
                                                        {
                                                            tList_Add(c7, list, ControlType);

                                                            if (c7.ToString() == dataLayout)
                                                                tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c7, list, ControlType);

                                                            if (c7.Controls.Count > 0)
                                                            {
                                                                #region Control8
                                                                foreach (Control c8 in c7.Controls)
                                                                {
                                                                    tList_Add(c8, list, ControlType);

                                                                    if (c8.ToString() == dataLayout)
                                                                        tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c8, list, ControlType);

                                                                    if (c8.Controls.Count > 0)
                                                                    {
                                                                        #region Control9
                                                                        foreach (Control c9 in c8.Controls)
                                                                        {
                                                                            tList_Add(c9, list, ControlType);

                                                                            if (c9.ToString() == dataLayout)
                                                                                tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c9, list, ControlType);

                                                                            if (c9.Controls.Count > 0)
                                                                            {
                                                                                #region Control10
                                                                                foreach (Control c10 in c9.Controls)
                                                                                {
                                                                                    tList_Add(c10, list, ControlType);

                                                                                    if (c10.ToString() == dataLayout)
                                                                                        tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c10, list, ControlType);

                                                                                    if (c10.Controls.Count > 0)
                                                                                    {
                                                                                        #region Control11
                                                                                        foreach (Control c11 in c10.Controls)
                                                                                        {
                                                                                            tList_Add(c11, list, ControlType);

                                                                                            if (c11.ToString() == dataLayout)
                                                                                                tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c11, list, ControlType);

                                                                                            if (c11.Controls.Count > 0)
                                                                                            {
                                                                                                #region Control12
                                                                                                foreach (Control c12 in c11.Controls)
                                                                                                {
                                                                                                    tList_Add(c12, list, ControlType);

                                                                                                    if (c12.ToString() == dataLayout)
                                                                                                        tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)c12, list, ControlType);

                                                                                                    //.....
                                                                                                }
                                                                                                #endregion // Control12
                                                                                            }
                                                                                        }
                                                                                        #endregion // Control11
                                                                                    }
                                                                                }
                                                                                #endregion // Control10
                                                                            }
                                                                        }
                                                                        #endregion // Control9
                                                                    }
                                                                }
                                                                #endregion // Control8
                                                            }
                                                        }
                                                        #endregion // Control7
                                                    }
                                                }
                                                #endregion // Control6
                                            }
                                        }
                                        #endregion // Control5
                                    }
                                }
                                #endregion // Control4
                            }

                        }
                        #endregion // Control3
                    }
                    #endregion // Control2
                }
            }
            */
            #endregion old code

            list.Sort();

            #endregion // Control1


            /* Example 
            string[] controls = new string[] { "DevExpress.XtraGrid.GridControl", 
                                               "DevExpress.XtraVerticalGrid.VGridControl",
                                               "DevExpress.XtraDataLayout.DataLayoutControl",
                                               "DevExpress.XtraTreeList.TreeList",
                                               "DevExpress.XtraEditors.DataNavigator"
                                             };
            List<string> list = new List<string>();
            t.Find_Control_List(tForm, list, controls);
            ****  * /
            
            string s = Kim + v.ENTER2;
            foreach (string value in list)
            {
                s = s + value + v.ENTER;
            }

            MessageBox.Show(s);
            */
        }

        private void Find_Control_List_(Control cntrl, List<string> list, string[] controlType,
             ref Int16 tabPageStatus, string tabPageName, ref Int16 cntrlLayer, ref Int16 tabPageLayer)
        {
            string dataLayout = "DevExpress.XtraDataLayout.DataLayoutControl";

            string cntrlName = cntrl.Name.ToString();
            
            //if (cntrlName != "")
            //    v.SQL = cntrlName + " ; " + cntrlLayer.ToString() + v.ENTER + v.SQL;

            // tabPageName Example : tTabPage_UST/OMS/FNS/MASRAFISLE
            //
            // 0 : tabpege arama yok 
            // 1 : tabpege aramaya başla
            // 2 : aranan tabPage bulundu
            // 3 : aranan tabPage işlemleri bitti 

            if (tabPageStatus == 2)
            {
                //
                //if ((cntrlName.IndexOf("tTabPage_") > -1) &&
                //    (cntrlName != tabPageName))

                // cntrlLayer   : bu layer sürekli artıyor ve düşüyor
                // tabPageLayer : aranan tabPage bulunduğunda tabPageLayer = cntrlLayer ile değeri saklanıyor
                // ne zaman bu cntrlLayer tabPageLayer e eşit veya alta düşerse işlem duruyor
                if (cntrlLayer <= tabPageLayer)
                    tabPageStatus = 3;
            }

            if (tabPageStatus == 1)
            {
                if (cntrlName == tabPageName)
                    tabPageStatus = 2;

                tabPageLayer = cntrlLayer;
            }

            if ((tabPageStatus == 0) || // tabPage koşulu yok ise
                (tabPageStatus == 2))   // tabPage bulunduysa
            {
                tList_Add(cntrl, list, controlType);

                if (cntrl.ToString() == dataLayout)
                    tLayoutControl_Add((DevExpress.XtraLayout.LayoutControl)cntrl, list, controlType);
            }

            if (cntrl.Controls.Count > 0)
            {
                cntrlLayer++;
                foreach (Control cntrl2 in cntrl.Controls)
                {
                    Find_Control_List_(cntrl2, list, controlType, ref tabPageStatus, tabPageName, ref cntrlLayer, ref tabPageLayer);
                }
                cntrlLayer--;
            }
        }

        private void tLayoutControl_Add(LayoutControl tLayoutControl, List<string> list, string[] ControlType)
        {
            //CheckedListBoxControl
            foreach (BaseLayoutItem item in ((DevExpress.XtraLayout.LayoutControl)tLayoutControl).Items)
            {
                tList_Add(item, list, ControlType);
            }
        }

        private void tList_Add(Control cntrl, List<string> list, string[] ControlType)
        {
            if (ControlType.Length == 0)
            {
                if (cntrl.Name.ToString() != string.Empty)
                    list.Add(cntrl.Name);
            }
            else
            {
                for (int t = 0; t < ControlType.Length; t++)
                {
                    if (cntrl.ToString() == ControlType[t])
                    {
                        if (cntrl.Name.ToString() != string.Empty)
                            list.Add(cntrl.Name.ToString());
                        break;
                    }
                }
            }
        }

        private void tList_Add(BaseLayoutItem cntrl, List<string> list, string[] ControlType)
        {
            if (ControlType.Length == 0)
            {
                if (cntrl.Name.ToString() != string.Empty)
                    list.Add(cntrl.Name);
            }
            else
            {
                for (int t = 0; t < ControlType.Length; t++)
                {
                    if (cntrl.ToString() == ControlType[t])
                    {
                        if (cntrl.Name.ToString() != string.Empty)
                            list.Add(cntrl.Name.ToString());
                        break;
                    }
                }
            }
        }

        #endregion Find_Control_List

        #endregion Get Functions

        #region *Diğer Yardımcı Fonksiyonlar

        #region Takipci
        public void Takipci(string function_name, string sub_function, char bayrak)
        {
            if ((v.Takip_Find == false) && (function_name.IndexOf("_Find") > 0)) return;

            if (v.Takip)
            {
                if (bayrak == '}') { --v.Takip_Adim; }

                if (v.Takip_Adim < 0) v.Takip_Adim = 0;

                if (v.Takip_Adim == 0) v.bosluk = "";
                if (v.Takip_Adim == 1) v.bosluk = "...";
                if (v.Takip_Adim == 2) v.bosluk = "......";
                if (v.Takip_Adim == 3) v.bosluk = ".........";
                if (v.Takip_Adim == 4) v.bosluk = "............";
                if (v.Takip_Adim == 5) v.bosluk = "...............";
                if (v.Takip_Adim == 6) v.bosluk = "..................";
                if (v.Takip_Adim == 7) v.bosluk = ".....................";
                if (v.Takip_Adim == 8) v.bosluk = "........................";
                if (v.Takip_Adim == 9) v.bosluk = "...........................";
                if (v.Takip_Adim == 10) v.bosluk = "..............................";
                if (v.Takip_Adim == 11) v.bosluk = ".................................";
                if (v.Takip_Adim == 12) v.bosluk = "....................................";


                if (bayrak == '{') { ++v.Takip_Adim; }

                v.Takip_Listesi = v.Takip_Listesi + v.ENTER + v.bosluk + function_name;

                if (sub_function != string.Empty)
                    v.Takip_Listesi = v.Takip_Listesi + ", " + sub_function;

                v.Takip_Listesi = v.Takip_Listesi + " " + bayrak;
            }
        }
        #endregion Takipci

        #region FormActiveControl

        /// parametre TableIPCode ise view nesnesi active edilecek 
        /// 
        public void tFormActiveView(Form tForm, string TableIPCode)
        {
            Control cntrl = null;
            string[] controls = new string[] { };
            cntrl = Find_Control_View(tForm, TableIPCode);

            if (cntrl != null)
            {
                if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    Control cntrl2 = cntrl;
                    controls = new string[] { "DevExpress.XtraEditors.TextEdit" };
                    cntrl = Find_Control(tForm, "", TableIPCode, controls);

                    //
                    // find yoksa yeniden gride odaklan
                    //if (cntrl == null) cntrl = cntrl2;
                    // bu seferde speed fieldler varsa onları atladığı için kapattım
                    //
                }
                // set et
                tFormActiveControl(tForm, cntrl);
            }
        }
        public void tFormActiveControl(Form tForm, string controlName)
        {
            Control cntrl = null;
            string[] controls = new string[] { };
            cntrl = Find_Control(tForm, controlName, "", controls);

            // set et
            tFormActiveControl(tForm, cntrl);
        }

        /// parametre TableIPCode ve FieldName ise view nesnesi active edilecek 
        /// 
        public void tFormActiveControl(Form tForm, string TableIPCode, string columnFrontName, string fieldName)
        {
            Control cntrl = null;
            string[] controls = new string[] { };

            //controlName = "Column_"
            //cntrl = Find_Control(tForm, "Column_" + sf_FieldName, sf_TableIPCode, controls);

            if (IsNotNull(TableIPCode))
            {
                if (IsNotNull(fieldName))
                    cntrl = Find_Control(tForm, columnFrontName + fieldName, TableIPCode, controls);

                if (IsNotNull(fieldName) == false)
                {
                    // önce Arama paneli varmı ona bak
                    cntrl = Find_Control(tForm, "textEdit_Find_" + AntiStr_Dot(TableIPCode));
                    // yoksa view nesnesini bul
                    if (cntrl == null)
                        cntrl = Find_Control_View(tForm, TableIPCode);
                }
            }

            // set et
            if (cntrl != null)
                tFormActiveControl(tForm, cntrl);
        }

        /// parametre bir control nesnesi ise kendisi set edilecek
        ///
        public void tFormActiveControl(Form tForm, Control cntrl)
        {
            if (v.con_CreatePopupContainer) return;
            if (cntrl != null)
            {
                
                tForm.ActiveControl = cntrl;
                tForm.ActiveControl.Focus();

                if (v.formLastActiveControl == null)
                    v.formLastActiveControl = cntrl;
            }
        }

        #endregion FormActiveControl

        #region ButtonEnabled and All

        public void ButtonEnabledAll(Form tForm, string TableIPCode, Boolean NewBool)
        {
            // gerek kalmadı

            // 53 yeni hesap
            //ButtonCaptionRefresh(tForm, TableIPCode, "simpleButton_yeni_hesap");
            // 54 yeni alt hesap
            //ButtonCaptionRefresh(tForm, TableIPCode, "simpleButton_yeni_alt_hesap");

            /*
            // 53 yeni hesap
            ButtonEnabled(tForm, TableIPCode, "simpleButton_yeni_hesap", NewBool);
            // 54 yeni alt hesap
            ButtonEnabled(tForm, TableIPCode, "simpleButton_yeni_alt_hesap", NewBool);
            // 58 yeni fiş
            ButtonEnabled(tForm, TableIPCode, "simpleButton_yeni_fis", NewBool);
            */
        }

        public void ButtonCaptionRefresh(Form tForm, string TableIPCode, string ButtonName)
        {
            Control cntrl = null;
            string[] controls = new string[] { };

            cntrl = Find_Control(tForm, ButtonName, TableIPCode, controls);

            if (cntrl != null)
            {
                // sakladığın caption yeniden kullan ( Vazgeç <<< Yeni Hesap )

                if (((DevExpress.XtraEditors.SimpleButton)cntrl).Tag != null)
                {

                    ((DevExpress.XtraEditors.SimpleButton)cntrl).Text =
                        ((DevExpress.XtraEditors.SimpleButton)cntrl).Tag.ToString();

                    if (((DevExpress.XtraEditors.SimpleButton)cntrl).Name == "simpleButton_yeni_hesap")
                    {
                        if (((DevExpress.XtraEditors.SimpleButton)cntrl).Text == "Vazgeç")
                            ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = Find_Glyph("YENIVAZGEÇ16");
                        else ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = Find_Glyph("YENIHESAP16");
                    }
                }
            }
        }

        public void ButtonEnabled(Form tForm, string TableIPCode, string ButtonName, Boolean NewBool)
        {
            Control cntrl = null;
            string[] controls = new string[] { };

            cntrl = Find_Control(tForm, ButtonName, TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Enabled = NewBool;
            }
        }

        #endregion ButtonEnabled and All

        #region PrevPage / NextPage

        public void PrevPage(Form tForm, string TableIPCode)
        {
            PageChange(tForm, TableIPCode, "PREVIOUS"); //previous
        }

        public void NextPage(Form tForm, string TableIPCode)
        {
            PageChange(tForm, TableIPCode, "NEXT");
        }

        public Control Find_Pages_Object(Form tForm, string TableIPCode)
        {
            Control viewcntrl = null;
            Control myParent1 = null;
            //Control myParent2 = null;

            if (IsNotNull(TableIPCode))
            {
                viewcntrl = Find_Control_View(tForm, TableIPCode);

                /// TableIPCode üzerinde bazen nesne adıda olabiliyor onun için
                /// tekrar isimdede arama yapılacak
                /// xxxx.Name = v.lyt_Name + RefId.ToString();  
                ///  
                if (viewcntrl == null)
                    viewcntrl = Find_Control(tForm, TableIPCode);
            }

            if (viewcntrl != null)
            {
                myParent1 = Find_TabControl(tForm, viewcntrl);
            }

            return myParent1;
        }

        private Control Find_TabControl(Form tForm, Control viewControl)
        {
            /// grid buldum, gridin hangi tabControl içinde olduğunu bulmam gerekir
            ///
            Control myParent1 = viewControl;
            Control myParent2 = null;

            /// önce gelenin kendisi kontrol ediliyor
            /// eğer aran tabcontrollerden biri ise aranan bulunmuştur
            if (myParent1 == null) return null;

            if (myParent1.GetType().ToString() == "DevExpress.XtraDataLayout.DataLayoutControl")
            {
                string TableIPCode = ((DevExpress.XtraDataLayout.DataLayoutControl)myParent1).AccessibleName;

                myParent1 = Find_Control(tForm, "navigationPane_" + AntiStr_Dot(TableIPCode));
                if (myParent1 == null) return null;
            }

            if (myParent1.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane") return myParent1;
            if (myParent1.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane") return myParent1;
            if (myParent1.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl") return myParent1;
            if (myParent1.GetType().ToString() == "DevExpress.XtraTab.XtraTabControl") return myParent1;
            if (myParent1.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
            {
                return myParent1.Parent;
            }

            /// eğer aranan tabcontrol bulunmadı ise sırayla üst hesapları kontrol edilecek
            /// takii boşa düşene kadar
            while (myParent1 != null)
            {
                if (myParent1.Parent != null)
                    myParent2 = myParent1.Parent;

                if (myParent2 == null) return myParent1;
                if (myParent2.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane") return myParent2;
                if (myParent2.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane") return myParent2;
                if (myParent2.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl") return myParent2;
                if (myParent2.GetType().ToString() == "DevExpress.XtraTab.XtraTabControl") return myParent2;

                if ((myParent2.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl") ||
                    (myParent2.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabNavigationPage") ||
                    (myParent2.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPage") ||
                    (myParent2.GetType().ToString() == "DevExpress.XtraTab.XtraTabPage")
                   )
                {
                    return myParent2.Parent;
                }

                myParent1 = myParent2.Parent;
            }

            return null;
        }

        public void PageChange(Form tForm, string TableIPCode, string Action)
        {
            Control myControl = null;

            myControl = Find_Pages_Object(tForm, TableIPCode);

            if (myControl != null)
                PageChange_(tForm, myControl, Action, TableIPCode);
        }

        private void PageChange_(Form tForm, Control myControl, string Action, string TableIPCode)
        {
            string myObjectName = string.Empty;
            string sayfa = "ortaSayfa";
            int oldIndex = -1;
            int newIndex = -1;
            int pageCount = -1;

            if (myControl != null)
                myObjectName = myControl.ToString();

            #region NEXT || LAST
            if ((myControl != null) && ((Action == "NEXT") || (Action == "LAST")))
            {
                if (myObjectName == "DevExpress.XtraBars.Navigation.TabPane")
                {
                    pageCount = ((DevExpress.XtraBars.Navigation.TabPane)myControl).Pages.Count;

                    if ((((DevExpress.XtraBars.Navigation.TabPane)myControl).Pages.Count - 1) >
                         ((DevExpress.XtraBars.Navigation.TabPane)myControl).SelectedPageIndex)
                    {
                        oldIndex = ((DevExpress.XtraBars.Navigation.TabPane)myControl).SelectedPageIndex;
                        // NEXT
                        newIndex = oldIndex + 1;

                        if (Action == "LAST")
                            newIndex = (pageCount - 1);

                        ((DevExpress.XtraBars.Navigation.TabPane)myControl).SelectedPageIndex = newIndex;
                    }
                }

                if (myObjectName == "DevExpress.XtraBars.Navigation.NavigationPane")
                {
                    pageCount = ((DevExpress.XtraBars.Navigation.NavigationPane)myControl).Pages.Count;

                    if ((((DevExpress.XtraBars.Navigation.NavigationPane)myControl).Pages.Count - 1) >
                         ((DevExpress.XtraBars.Navigation.NavigationPane)myControl).SelectedPageIndex)
                    {
                        oldIndex = ((DevExpress.XtraBars.Navigation.NavigationPane)myControl).SelectedPageIndex;
                        // NEXT
                        newIndex = oldIndex + 1;

                        if (Action == "LAST")
                            newIndex = (pageCount - 1);

                        ((DevExpress.XtraBars.Navigation.NavigationPane)myControl).SelectedPageIndex = newIndex;
                    }
                }

                if (myObjectName == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                {
                    pageCount = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).Items.Count;

                    if ((((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).Items.Count - 1) >
                         ((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).SelectedTabIndex)
                    {
                        oldIndex = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).SelectedTabIndex;
                        // NEXT
                        newIndex = oldIndex + 1;

                        if (Action == "LAST")
                            newIndex = (pageCount - 1);

                        ((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).SelectedTabIndex = newIndex;
                    }
                }

                if (myObjectName == "DevExpress.XtraTab.XtraTabControl")
                {
                    pageCount = ((DevExpress.XtraTab.XtraTabControl)myControl).TabPages.Count;

                    if ((((DevExpress.XtraTab.XtraTabControl)myControl).TabPages.Count - 1) >
                         ((DevExpress.XtraTab.XtraTabControl)myControl).TabIndex)
                    {
                        oldIndex = ((DevExpress.XtraTab.XtraTabControl)myControl).TabIndex;
                        // NEXT
                        newIndex = oldIndex + 1;

                        if (Action == "LAST")
                            newIndex = (pageCount - 1);

                        ((DevExpress.XtraTab.XtraTabControl)myControl).TabIndex = newIndex;
                    }
                }

                if ((newIndex == (pageCount - 1)) ||
                    (newIndex == -1))
                    sayfa = "sonSayfa";

            }
            #endregion NEXT

            #region PREVIOUS || FIRST
            if ((myControl != null) && ((Action == "PREVIOUS") || (Action == "FIRST")))
            {
                if (myObjectName == "DevExpress.XtraBars.Navigation.TabPane")
                {
                    if (((DevExpress.XtraBars.Navigation.TabPane)myControl).SelectedPageIndex > 0)
                    {
                        oldIndex = ((DevExpress.XtraBars.Navigation.TabPane)myControl).SelectedPageIndex;
                        // PREVIOUS 
                        newIndex = oldIndex - 1;
                        if (Action == "FIRST") newIndex = 0;

                        ((DevExpress.XtraBars.Navigation.TabPane)myControl).SelectedPageIndex = newIndex;
                    }
                }

                if (myObjectName == "DevExpress.XtraBars.Navigation.NavigationPane")
                {
                    if (((DevExpress.XtraBars.Navigation.NavigationPane)myControl).SelectedPageIndex > 0)
                    {
                        oldIndex = ((DevExpress.XtraBars.Navigation.NavigationPane)myControl).SelectedPageIndex;
                        // PREVIOUS 
                        newIndex = oldIndex - 1;
                        if (Action == "FIRST") newIndex = 0;

                        ((DevExpress.XtraBars.Navigation.NavigationPane)myControl).SelectedPageIndex = newIndex;
                    }
                }

                if (myObjectName == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                {
                    if (((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).SelectedTabIndex > 0)
                    {
                        oldIndex = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).SelectedTabIndex;
                        // PREVIOUS 
                        newIndex = oldIndex - 1;
                        if (Action == "FIRST") newIndex = 0;

                        ((DevExpress.XtraBars.Ribbon.BackstageViewControl)myControl).SelectedTabIndex = newIndex;
                    }
                }

                if (myObjectName == "DevExpress.XtraTab.XtraTabControl")
                {
                    if (((DevExpress.XtraTab.XtraTabControl)myControl).TabIndex > 0)
                    {
                        oldIndex = ((DevExpress.XtraTab.XtraTabControl)myControl).TabIndex;
                        // PREVIOUS 
                        newIndex = oldIndex - 1;
                        if (Action == "FIRST") newIndex = 0;

                        ((DevExpress.XtraTab.XtraTabControl)myControl).TabIndex = newIndex;
                    }
                }

                if (newIndex == 0)
                    sayfa = "ilkSayfa";


            }
            #endregion PREVIOUS

            //simpleButton_sihirbaz_geri
            //simpleButton_sihirbaz_sonra
            //simpleButton_sihirbaz_devam

            Control cntrl = null;
            string[] controls = new string[] { };
            cntrl = Find_Control(tForm, "simpleButton_sihirbaz_geri", TableIPCode, controls);

            if (cntrl != null)
            {
                if (sayfa == "ilkSayfa")
                    ((DevExpress.XtraEditors.SimpleButton)cntrl).Enabled = false;
                else ((DevExpress.XtraEditors.SimpleButton)cntrl).Enabled = true;
            }

            cntrl = Find_Control(tForm, "simpleButton_sihirbaz_sonra", TableIPCode, controls);

            if (cntrl != null)
            {
                if (sayfa == "sonSayfa")
                    ((DevExpress.XtraEditors.SimpleButton)cntrl).Enabled = false;
                else ((DevExpress.XtraEditors.SimpleButton)cntrl).Enabled = true;
            }

            cntrl = Find_Control(tForm, "simpleButton_sihirbaz_devam", TableIPCode, controls);

            if (cntrl != null)
            {
                if (sayfa == "sonSayfa")
                {
                    if (((DevExpress.XtraEditors.SimpleButton)cntrl).Text != "Kaydet")
                    {
                        ((DevExpress.XtraEditors.SimpleButton)cntrl).Text = "Kaydet";
                        ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = Find_Glyph("KAYDET16");
                        //((DevExpress.XtraEditors.SimpleButton)cntrl).Appearance.ForeColor = Color.Red;
                    }
                }
                else
                {
                    if (((DevExpress.XtraEditors.SimpleButton)cntrl).Text != "Devam")
                    {
                        ((DevExpress.XtraEditors.SimpleButton)cntrl).Text = "Devam";
                        ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = Find_Glyph("SIHIRBAZDEVAM16");
                    }
                }
            }


            /// dataWizard üzerindeki mesaj labelleri
            /// 
            #region labelControls
            if (oldIndex >= 0)
            {
                Control oldLabel = Find_Control(tForm, "labelControl_Step_" + oldIndex);
                if (oldLabel != null)
                {
                    // 
                    // old >> labelControl_Step1
                    // 
                    ((DevExpress.XtraEditors.LabelControl)oldLabel).Appearance.BackColor = System.Drawing.SystemColors.ControlLight;
                    ((DevExpress.XtraEditors.LabelControl)oldLabel).Appearance.ForeColor = System.Drawing.SystemColors.ControlDarkDark;

                    Control newLabel = Find_Control(tForm, "labelControl_Step_" + newIndex);

                    if (newLabel != null)
                    {
                        // 
                        // new >> labelControl_Step1
                        // 
                        ((DevExpress.XtraEditors.LabelControl)newLabel).Appearance.BackColor = System.Drawing.SystemColors.InactiveCaption;
                        ((DevExpress.XtraEditors.LabelControl)newLabel).Appearance.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    }
                    // işlemi bitir
                    oldIndex = -1;
                    newIndex = -1;
                }
            }
            #endregion labelControls
        }

        #endregion PrevPage / NextPage

        #region RibbonPagesSet
        public void RibbonPagesSet(Control ribbon, string pName)
        {
            if (pName.IndexOf("item_") == -1)
                pName = "item_" + pName;

            foreach (DevExpress.XtraBars.Ribbon.RibbonPage page in ((DevExpress.XtraBars.Ribbon.RibbonControl)ribbon).Pages)
            {
                page.Visible = (page.Name.ToString() == pName);
            }
        }
        #endregion

        #region MSSQL_Server_Tarihi
        public void MSSQL_Server_Tarihi()
        {
            /*
            if (v.SAAT_KISA != "")
            {
                int t1saat = Convert.ToInt32(v.SAAT_KISA.Substring(0,2));
                int t2saat = Convert.ToInt32(DateTime.Now.ToShortTimeString().Substring(0,2));

                int t1dakika = Convert.ToInt32(v.SAAT_KISA.Substring(3, 2));
                int t2dakika = Convert.ToInt32(DateTime.Now.ToShortTimeString().Substring(3, 2));
                int fark = t2dakika - t1dakika;
                if (fark < 11) return; // 11 dakikadan küçükse bir daha sorgulama
            }
            */

            string ySql =
              @"  select 
               GETDATE() DB_TARIH 
             , CONVERT(varchar(10), GETDATE(), 104) TARIH 
             , CONVERT(varchar(10), GETDATE(), 102) YILAYGUN 
             , CONVERT(varchar(10), GETDATE(), 8) UZUN_SAAT 
             , CONVERT(varchar(20), GETDATE(), 14) UZUN_SAAT2 
             , YEAR(GETDATE()) YIL 
             , MONTH(GETDATE()) AY 
             , DAY(GETDATE()) GUN 
             , DATEPART(weekday, GETDATE()) HAFTA_GUN
            --3S_
            " + v.ENTER;
            DataSet ds = new DataSet();
            /*
 
DECLARE @tarih DATETIME=GETDATE()
SELECT 'Onceki Ayın Son Gunu' Aciklama, CONVERT(VARCHAR(10),DATEADD(dd,-(DAY(@tarih)),@tarih),112)  Tarih
UNION ALL
SELECT 'Ayın İlk Günü',                 CONVERT(VARCHAR(10),DATEADD(dd,-(DAY(@tarih)-1),@tarih),112) AS Date_Value
UNION ALL
SELECT 'Bugunun Tarihi',                CONVERT(VARCHAR(10),@tarih,112) AS Date_Value
UNION ALL
SELECT 'Ayın Son Günü',                 CONVERT(VARCHAR(10),DATEADD(dd,-(DAY(DATEADD(mm,1,@tarih))),DATEADD(mm,1,@tarih)),112) 
UNION ALL
SELECT 'Sonraki Ayın İlk Günü',         CONVERT(VARCHAR(10),DATEADD(dd,-(DAY(DATEADD(mm,1,@tarih))-1),DATEADD(mm,1,@tarih)),112) 
UNION ALL
SELECT 'Haftanın İlk Günü',             DATEADD(ww, DATEDIFF(ww,0,GETDATE()), 0)
UNION ALL
SELECT 'Sonraki Haftanın İlk Günü',     DATEADD(ww, DATEDIFF(ww,0,GETDATE())+1, 0)
UNION ALL
SELECT 'Yılın İlk Günü',                DATEADD(yy, DATEDIFF(yy,0,getdate()), 0)
UNION ALL
SELECT 'Yılın Son Günü',                DATEADD(dd,-1,DATEADD(yy,0,DATEADD(yy,DATEDIFF(yy,0,getdate())+1,0)))
            */

            if (SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref ySql, "", "MSSQL Server Tarihi"))
            {
                v.BUGUN_TARIH = Convert.ToDateTime(ds.Tables[0].Rows[0]["TARIH"].ToString());
                v.DONEM_BASI_TARIH = new DateTime(v.BUGUN_TARIH.Year, 1, 1);  
                v.DONEM_BITIS_TARIH = new DateTime(v.BUGUN_TARIH.Year, 12, 31);
                v.BU_HAFTA_BASI_TARIH = v.BUGUN_TARIH; //???
                v.BU_HAFTA_SONU_TARIH = v.BUGUN_TARIH; //???
                v.BIR_HAFTA_ONCEKI_TARIH = DateTime.Now.AddDays(-7);
                v.IKI_HAFTA_ONCEKI_TARIH = DateTime.Now.AddDays(-14);
                v.ON_GUN_ONCEKI_TARIH = DateTime.Now.AddDays(-10);
                v.GECEN_AY_BUGUN = DateTime.Now.AddMonths(-1);
                v.GELECEK_AY_BUGUN = DateTime.Now.AddMonths(1);

                // Geçen Yil_Ay
                v.BUGUN_GUN = v.GECEN_AY_BUGUN.Day;
                v.BUGUN_AY = v.GECEN_AY_BUGUN.Month;
                v.BUGUN_YIL = v.GECEN_AY_BUGUN.Year;

                if (v.BUGUN_AY < 10)
                     v.GECEN_YILAY = Convert.ToInt32(v.BUGUN_YIL.ToString() + '0' + v.BUGUN_AY.ToString());
                else v.GECEN_YILAY = Convert.ToInt32(v.BUGUN_YIL.ToString() + v.BUGUN_AY.ToString());

                // Gelecek Yil_Ay
                v.BUGUN_GUN = v.GELECEK_AY_BUGUN.Day;
                v.BUGUN_AY = v.GELECEK_AY_BUGUN.Month;
                v.BUGUN_YIL = v.GELECEK_AY_BUGUN.Year;

                if (v.BUGUN_AY < 10)
                     v.GELECEK_YILAY = Convert.ToInt32(v.BUGUN_YIL.ToString() + '0' + v.BUGUN_AY.ToString());
                else v.GELECEK_YILAY = Convert.ToInt32(v.BUGUN_YIL.ToString() + v.BUGUN_AY.ToString());

                // Bugün Yil_Ay
                v.BUGUN_GUN = v.BUGUN_TARIH.Day;
                v.BUGUN_AY = v.BUGUN_TARIH.Month;
                v.BUGUN_YIL = v.BUGUN_TARIH.Year;

                if (v.BUGUN_AY < 10)
                     v.BUGUN_YILAY = Convert.ToInt32(v.BUGUN_YIL.ToString() + '0' + v.BUGUN_AY.ToString());
                else v.BUGUN_YILAY = Convert.ToInt32(v.BUGUN_YIL.ToString() + v.BUGUN_AY.ToString());

                v.TARIH_SAAT = ds.Tables[0].Rows[0]["DB_TARIH"].ToString();
                v.SAAT_KISA = ds.Tables[0].Rows[0]["UZUN_SAAT"].ToString().Substring(0, 5);
                v.SAAT_UZUN = ds.Tables[0].Rows[0]["UZUN_SAAT"].ToString();
                v.SAAT_UZUN2 = ds.Tables[0].Rows[0]["UZUN_SAAT2"].ToString();

                //-- BU_HAFTA_BASI_TARIH
                string gun = v.BUGUN_TARIH.DayOfWeek.ToString();
                byte g = 0;

                if (gun == "Sunday") g = 1;
                if (gun == "Monday") g = 7;
                if (gun == "Tuesday") g = 6;
                if (gun == "Wednesday") g = 5;
                if (gun == "Thursday") g = 4;
                if (gun == "Friday") g = 3;
                if (gun == "Saturday") g = 2;

                v.BU_HAFTA_BASI_TARIH = v.BUGUN_TARIH.Date.AddDays(-1 * (7 - g));

                //-- BU_HAFTA_SONU_TARIH
                gun = v.BUGUN_TARIH.DayOfWeek.ToString();

                if (gun == "Sunday") g = 0;
                if (gun == "Monday") g = 6;
                if (gun == "Tuesday") g = 5;
                if (gun == "Wednesday") g = 4;
                if (gun == "Thursday") g = 3;
                if (gun == "Friday") g = 2;
                if (gun == "Saturday") g = 1;

                v.BU_HAFTA_SONU_TARIH = v.BUGUN_TARIH.Date.AddDays(g);

                Tarihin_Ayin_IlkGunu_SonGunu(v.BUGUN_TARIH, ref v.BU_AY_BASI_TARIH, ref v.BU_AY_SONU_TARIH);
            }

            ds.Dispose();
        }
        #endregion MSSQL_Server_Tarihi

        #region Setting Table Read
        public void read_Settings()
        {
            /*
            string Mesaj =
                 "User Id : " + v.tUser.UserId.ToString() + v.ENTER
               + "User Name : " + v.tUser.FullName + v.ENTER
               + "Firm Id : " + v.tMainFirm.FirmId.ToString() + v.ENTER
               + "Computer Name : " + v.tComputer.PcName + v.ENTER
               + "Computer MAC : " + v.tComputer.Network_MACAddress;
            MessageBox.Show(Mesaj);
            */
            string fields = @"
       [Id]
      ,[IsActive]
      ,[SettingTypeId]
      ,[FirmId]
      ,[PcName]
      ,[NetworkMacAddress]
      ,[UserId]
      ,[GroupNoTypeId]
      ,[SettingNo]
      ,[About]
      ,[DefaultTypeId]
      ,[DefaultBool]
      ,[DefaultNumeric]
      ,[DefaultText]
      ,[DefaultInt] ";

            string Sql = // Tablo varsa Select çalışsın
                   " IF EXISTS (Select * From sys.objects WHERE name = 'Settings') "
                 + " begin "
                 + "     Select " + fields + " from Settings " + v.ENTER
                 + "     Where UserId = " + v.tUser.UserId.ToString() + v.ENTER;
            
            Sql += "     union all " + v.ENTER
                 + "     Select " + fields + " from Settings " + v.ENTER
                 + "     Where  PcName = '" + v.tComputer.PcName + "' "
                 + "     and    NetworkMacAddress = '" + v.tComputer.Network_MACAddress + "' " + v.ENTER;

            Sql += "     union all "
                 + "     Select " + fields + " from Settings " + v.ENTER
                 + "     Where  FirmId = " + v.tMainFirm.FirmId + v.ENTER
                 + " end -- if exists ";
            try
            {
                if (v.ds_Settings != null)
                    TableRemove(v.ds_Settings);

                SQL_Read_Execute(v.dBaseNo.Project, v.ds_Settings, ref Sql, "Settings", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings tablosu okunamadı. \n" + ex.Message, "Settings Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                v.ds_Settings = null;
                //throw;
            }
        }
        #endregion Setting Table Read

        #region Settings get value
        public bool getSettingsBoolValue(Int16 SettingNo)
        {
            bool value = false;

            DataRow row = getSettingsRow(SettingNo);

            if (row != null)
                value = Convert.ToBoolean(row["DefaultBool"]);
            else value = true; // default 

            return value;
        }
        private DataRow getSettingsRow(Int16 SettingNo)
        {
            DataRow row = null;

            if (IsNotNull(v.ds_Settings))
            {
                int count = v.ds_Settings.Tables[0].Rows.Count;

                for (int i = 0; i < count; i++)
                {
                    if (Convert.ToInt32(v.ds_Settings.Tables[0].Rows[i]["SettingNo"]) == SettingNo)
                    {
                        row = v.ds_Settings.Tables[0].Rows[i];
                        break;
                    }
                }
            }
            return row;
        }

        #endregion Settings get value

        #region MsExeUpdates Read

        public void read_MsExeUpdates(v.tUserType userType)
        {
            string ySql =
              @"  select top 1 
               upd.[Id]
             , upd.[IsActive]
             , upd.[RecordDate]
             , upd.[ExeName]
             , upd.[VersionNo]
             , upd.[PacketName]
             , upd.[About]
            from MsExeUpdates upd 
            where upd.IsActive = 1
            order by upd.RecordDate desc ";

            DataSet ds = new DataSet();

            if (userType == v.tUserType.EndUser)
            {
                if (SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref ySql, "", "MsExeUpdates"))
                {
                    v.tExeAbout.ftpVersionNo = ds.Tables[0].Rows[0]["VersionNo"].ToString();
                    v.tExeAbout.ftpFileName = ds.Tables[0].Rows[0]["ExeName"].ToString();
                    v.tExeAbout.ftpPacketName = ds.Tables[0].Rows[0]["PacketName"].ToString();
                }
            }
            if (userType == v.tUserType.TesterUser)
            {
                if (SQL_Read_Execute(v.dBaseNo.Manager, ds, ref ySql, "", "MsExeUpdates"))
                {
                    v.tExeAbout.ftpVersionNo = ds.Tables[0].Rows[0]["VersionNo"].ToString();
                    v.tExeAbout.ftpFileName = ds.Tables[0].Rows[0]["ExeName"].ToString();
                    v.tExeAbout.ftpPacketName = ds.Tables[0].Rows[0]["PacketName"].ToString();
                }
            }

            ds.Dispose();
        }

        #endregion MsExeUpdates

        #region MsFileUpdates Read
        public void read_MsFileUpdates()
        {
            //v.active_DB.projectDBaseNo
            //v.dBaseNo.Local,

            v.dBaseNo dBaseNo = v.active_DB.projectDBaseNo;
            if ((v.SP_Firm_SectorTypeId == 211) || 
                 (v.SP_Firm_SectorTypeId == 212) || 
                 (v.SP_Firm_SectorTypeId == 213))
            {
                if (v.active_DB.localDbUses)
                    dBaseNo = v.dBaseNo.Local;
            }

            bool onay = preparingCreateTable(dBaseNo, "dbo", "FileUpdates", 211);

            /// UstadCrmV1 ise iptal, file kontrole gerek yok
            if (v.SP_Firm_SectorTypeId == 5) onay = false;

            if (onay) 
                fileUpdatesChecked();
        }
        #endregion MsFileUpdates

        #region MenuCodeChecked
        public void menuCodeChecked()
        {
            /* sil
            if ((v.active_DB.localDbUses == false) &&
                ((v.SP_Firm_SectorTypeId == 211) ||
                 (v.SP_Firm_SectorTypeId == 212) ||
                 (v.SP_Firm_SectorTypeId == 213)))
                    v.tMainFirm.MenuCode = "UST/MEB/MTS/MAIN";
            */
            if (v.active_DB.localDbUses == false)
            {
                /// src ve işmak için ayrıca belirlenecek
                //if ((v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimSrc) ||
                //    (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimIsmak))
                //     v.tMainFirm.MenuCode = "UST/MEB/MTS/MAIN";

                if (((v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimMtsk) ||
                     (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadMtsk)) &&
                     (v.tMainFirm.MenuCode != "UST/MEB/MTS/MAIN"))
                     v.tMainFirm.MenuCode = "UST/MEB/MTS/MAIN";
            }

        }
        #endregion MenuCodeChecked

        #region YilAy - Mali Dönem Read
        public async Task DonemTipiYilAyRead()
        {
            if (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadCrm) return;
            if (v.active_DB.localDbUses) return;

            string tableName = "";
            string Sql = "";

            TableRemove(v.ds_DonemTipiList);
            
            if ((v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadMtsk) ||
                (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimMtsk) ||
                (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimSrc) ||
                (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimIsmak))
            {
                tableName = "MtskDonemTipi";
                Sql = " Select * from [Lkp].[" + tableName + "] order by Id desc ";
            }
            else if (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadSrc)
            {
                tableName = "SrcDonemTipi";
                Sql = " Select * from [Lkp].[" + tableName + "] order by Id desc ";
            }
            else
            {
                tableName = "BelgeDonemTipi";
                Sql = " Select * from [Lkp].[" + tableName + "] order by Id desc ";
            }
            
            v.dBaseNo dBaseNo = v.dBaseNo.Project;
            SQL_Read_Execute(dBaseNo, v.ds_DonemTipiList, ref Sql, tableName, "");
        }

        public void TestRead()
        {
            DataSet ds = new DataSet();

            string tableName = "UstadFirms";
            string Sql = " Select * from [dbo].[UstadFirms] with (nolock) where FirmGUID = '60e9532c-bff2-4772-a593-ba5c7ec7e895' ";
            Sql = " Select * from [dbo].[UstadFirms] with (nolock) where FirmLongName like '%HEDEF%' ";

            v.dBaseNo dBaseNo = v.dBaseNo.UstadCrm;
            SQL_Read_Execute(dBaseNo, ds, ref Sql, tableName, "");

            string a = "durdur";
            
        }

        #endregion YilAy - Mali Dönem Read

        #region DBUpdates Veri Aktarımı IsActive Set OFF

        public void DBUpdatesDataTransferOff()
        {
            //if (v.tMainFirm.SectorTypeId == 201) // Mtsk
            // Ustad Crm değilse 
            if (v.tMainFirm.SectorTypeId != 5) /// Ustad Crm
            {
                // IsActive 1 ise IsActive 0 olacak
                string Sql = " Update [dbo].[DbUpdates] set IsActive = 0 Where MsDbUpdateId = -1 and UpdateTypeId = 13 and DBaseNoTypeId = 6 and IsActive = 1 ";

                vTable vt = new vTable();
                vt.DBaseNo = v.dBaseNo.Project;
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.DBaseName = v.active_DB.projectDBName;
                vt.msSqlConnection = v.active_DB.projectMSSQLConn;

                DataSet ds = new DataSet();

                Sql_ExecuteNon(ds, ref Sql, vt);

                ds.Dispose();
            }
        }

        public string DBUpdatesDataTransferOnSql()
        {
            return " Update [dbo].[DbUpdates] set IsActive = 1 Where MsDbUpdateId = -1 and UpdateTypeId = 13 and DBaseNoTypeId = 6 ";
        }

        #endregion DBUpdates Veri Aktarımı IsActive Set OFF

        #region tGotoRecord
        public Boolean tGotoRecord(Form tForm, DataSet dsData, string TableIPCode, string FieldName, string Value, int position)// string TalepEden)
        {
            Boolean snc = false;

            if ((tForm == null) ||
                (IsNotNull(TableIPCode) == false) ||
                (IsNotNull(FieldName) == false) ||
                (IsNotNull(Value) == false))
            {
                return snc;
            }
                       
            // Her hangi bir id ye ulaşmak için  ...
            // DataRow[] row = dsData.Tables[0].Select(FieldName + "=" + Value);
            // ----

            DataNavigator tDataNavigator = null;

            int i = 0;
            // position daha önce tespit edilmemişse 
            if (position < 0)
            {
                if (dsData == null)
                    Find_DataSet(tForm, ref dsData, ref tDataNavigator, TableIPCode);
                //dsData = Find_DataSet(tForm, "", TableIPCode, "");

                if (IsNotNull(dsData))
                    i = Find_GotoRecord(dsData, FieldName, Value);
            }
            else i = position;

            if (i > -1)
            {
                if (tDataNavigator == null)
                    tDataNavigator = Find_DataNavigator(tForm, TableIPCode);

                if (tDataNavigator != null)
                {
                    // burada atama yapılıyor fakat position değişmiyor
                    tDataNavigator.Position = i;
                    tDataNavigator.Tag = i;
                    v.con_SubView_FIRST_POSITION = i;
                    // ---

                    v.con_GotoRecord = string.Empty;
                    v.con_GotoRecord_TableIPCode = string.Empty;
                    v.con_GotoRecord_FName = string.Empty;
                    v.con_GotoRecord_Value = string.Empty;
                    v.con_GotoRecord_Position = -1;
                    snc = true;
                }
            }
            else
            {
                // Eğer halen -1 ise form DialogForm olduğu zaman çalışmıyor
                // Onun için FormLoad sırasında bu işlemleri tekrar yapmak gerekiyor
                v.con_GotoRecord = "ON";
                v.con_GotoRecord_TableIPCode = TableIPCode;
                v.con_GotoRecord_FName = FieldName;
                v.con_GotoRecord_Value = Value;
                v.con_GotoRecord_Position = position;
                snc = false;
            }

            return snc;
        }
        #endregion GotoRecord

        #region SYS_Types_List

        public async Task SYS_Types_Read()
        {
            /// 1. işlem
            ///
            SYS_Types_Names_Read();

            /// 2. işlem
            /// 
            //SYS_Variables_Read();
            
            /// 2022.02.26 HP_FIRM neden kullanlanılıyor hatırlamıyorum
            /// 
            ///HP_FIRM_Preparing();

            /// 4. İşlem 
            /// exe update varmı
            //HP_UPDATE_Preparing();

        }

        private void SYS_Types_Names_Read()
        {
            tSQLs sql = new tSQLs();

            /// 1. adım 
            string SysTypesSql = "";
            string MsTypesSql = "";

            sql.SQL_Types_List(ref SysTypesSql, ref MsTypesSql);

            if (IsNotNull(SysTypesSql))
                SQL_Read_Execute(v.dBaseNo.Manager, v.ds_TypesList, ref SysTypesSql, "SYS_TYPES_LIST", "");
              //SQL_Read_Execute(v.dBaseNo.Project, v.ds_TypesList, ref SysTypesSql, "SYS_TYPES_LIST", "");

            if (IsNotNull(MsTypesSql))
                SQL_Read_Execute(v.dBaseNo.Manager, v.ds_MsTypesList, ref MsTypesSql, "MS_TYPES_LIST", "");

            /// amaç : tüm types_name isimlerini 

            string rName = "";
            string tName = "";
            v.ds_TypesNames = "";
            v.ds_MsTypesNames = "";

            int i1 = 0;
            if (v.ds_TypesList != null)
            {
                if (v.ds_TypesList.Tables.Count > 0)
                    i1 = v.ds_TypesList.Tables[0].Rows.Count;

                for (int i = 0; i < i1; i++)
                {
                    rName = v.ds_TypesList.Tables[0].Rows[i]["TYPES_NAME"].ToString();
                    if (rName != tName)
                    {
                        v.ds_TypesNames = v.ds_TypesNames + rName + ";" + v.ENTER;
                        tName = rName;
                    }
                }
            }

            i1 = 0;
            if (v.ds_MsTypesList != null)
            {
                if (v.ds_MsTypesList.Tables.Count > 0)
                    i1 = v.ds_MsTypesList.Tables[0].Rows.Count;

                for (int i = 0; i < i1; i++)
                {
                    rName = v.ds_MsTypesList.Tables[0].Rows[i]["TYPES_NAME"].ToString();
                    if (rName != tName)
                    {
                        v.ds_MsTypesNames = v.ds_MsTypesNames + rName + ";" + v.ENTER;
                        tName = rName;
                    }
                }
            }

        }

        public void SYS_Variables_Read()
        {
            /// SYS_VARIABLES list
            ///
            /*
            if (v.active_DB.managerDBName != "SystemMS")
            {
                tSQLs sql = new tSQLs();

                string Sql = sql.SQL_SYS_Variables_List(v.active_DB.projectDBType);

                if (IsNotNull(Sql))
                    SQL_Read_Execute(v.dBaseNo.Project, v.ds_Variables, ref Sql, "SYS_VARIABLES_LIST", "");
            }
            */
        }
        
        private string Sql_HP_COMPS()
        {
            string s = "";

            if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                s = @"
            if ( select count(*) ADET from HP_COMPS
                 where ID > 0 
                 and  NETWORK_MACADDRESS = '" + v.tComp.SP_COMP_MACADDRESS + @"'
                 and ( ISACTIVE <> " + v.tComp.SP_COMP_ISACTIVE.ToString() + @"
                   or  SYSTEM_NAME <> '" + v.tComp.SP_COMP_SYSTEM_NAME + @"' 
                   or  COMP_FIRM_GUID <> '" + v.tComp.SP_COMP_FIRM_GUID + @"' 
            )) = 1
            begin

              UPDATE HP_COMPS
              SET ISACTIVE = " + v.tComp.SP_COMP_ISACTIVE.ToString() + @"
                 ,COMP_FIRM_GUID = '" + v.tComp.SP_COMP_FIRM_GUID + @"'
                 ,SYSTEM_NAME = '" + v.tComp.SP_COMP_SYSTEM_NAME + @"'
              WHERE NETWORK_MACADDRESS = '" + v.tComp.SP_COMP_MACADDRESS + @"'
            end  

            if ( select count(*) ADET from HP_COMPS
                 where NETWORK_MACADDRESS = '" + v.tComp.SP_COMP_MACADDRESS + @"'
            ) = 0
            begin
              INSERT INTO HP_COMPS
                (SYS_ID
                ,ISACTIVE
                ,COMP_FIRM_GUID
                ,SYSTEM_NAME
                ,NETWORK_MACADDRESS
                ,PROCESSOR_ID
                ,REC_DATE)
              VALUES
                (" + v.tComp.SP_COMP_ID.ToString() + @"
                ,1
                ,'" + v.tComp.SP_COMP_FIRM_GUID + @"'
                ,'" + v.tComp.SP_COMP_SYSTEM_NAME + @"'
                ,'" + v.tComp.SP_COMP_MACADDRESS + @"'
                ,'" + v.tComp.SP_COMP_PROCESSOR_ID + @"'
                ,getdate()
                )
            end    ";

            return s;
        }


        private string Sql_HP_USERS()
        {

            string s = "";

            if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                s = @"
            if ( select count(*) ADET from HP_USERS
                 where ID > 0 
                 and  USER_EMAIL = '" + v.tUser.eMail + @"'
                 and ( ISACTIVE <> " + v.tUser.IsActive.ToString() + @"
                   or  USER_FIRM_GUID <> '" + v.tUser.UserFirmGUID + @"'
                   or  USER_FIRSTNAME <> '" + v.tUser.FirstName + @"' 
                   or  USER_LASTNAME <> '" + v.tUser.LastName + @"'
            )) = 1
            begin
              UPDATE HP_USERS
              SET ISACTIVE = " + v.tUser.IsActive.ToString() + @"
                 ,USER_FIRM_GUID = '" + v.tUser.UserFirmGUID + @"'
                 ,USER_FIRSTNAME = '" + v.tUser.FirstName + @"'
                 ,USER_LASTNAME = '" + v.tUser.LastName + @"' 
              WHERE USER_EMAIL = '" + v.tUser.eMail + @"'
            end

            if ( select count(*) ADET from HP_USERS
                 where USER_EMAIL = '" + v.tUser.eMail + @"'
            ) = 0
            begin
              INSERT INTO HP_USERS
                (SYS_ID
                ,USER_FIRM_GUID
                ,ISACTIVE
                ,USER_GUID
                ,USER_FULLNAME
                ,USER_FIRSTNAME
                ,USER_LASTNAME
                ,USER_EMAIL
                ,REC_DATE)
              VALUES
                (" + v.tUser.UserId.ToString() + @"
                ,'" + v.tUser.UserFirmGUID + @"'
                ,1
                ,'" + v.tUser.UserGUID + @"'
                ,'" + v.tUser.FullName + @"'
                ,'" + v.tUser.FirstName + @"'
                ,'" + v.tUser.LastName + @"'
                ,'" + v.tUser.eMail + @"'
                ,getdate()
                )
            end     ";

            return s;
        }

        private string Sql_HP_FIRM_Insert(
            Int16 isActive,
            string myGuid,
            string localTd,
            string firmId,
            string parentId,
            string firmName)
        {
            string s = "";

            if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                s = @"
                if ( Select count(*) ADET from [HP_FIRMS] where 0 = 0 
                     and [FIRM_GUID] = '" + myGuid + @"' 
                ) = 0 
                begin 
                  insert into [HP_FIRMS] ( 
                       ISACTIVE, 
                       REC_DATE, 
                       FIRM_GUID, 
                       LOCAL_TD, 
                       LOCAL_ID, 
                       PARENT_ID, 
                       FIRM_NAME,
                       HP_NAME
                       ) values ( 1 , getdate() 
                      , '" + myGuid + @"'
                      , " + localTd + @"
                      , " + firmId + @"
                      , " + parentId + @"
                      , '" + firmName + @"'
                      , '" + firmName + @"' ) 
                end  

                if ( select count(*) ADET from HP_FIRMS
                     where ID > 0 
                     and FIRM_GUID = '" + myGuid + @"'
                     and ( ISACTIVE <> " + isActive.ToString() + @"
                       or  LOCAL_TD <> " + localTd + @" 
                       or  PARENT_ID <> " + parentId + @"
                       or  FIRM_NAME <> '" + firmName + @"' 
                )) = 1
                begin
                  UPDATE HP_FIRMS
                    SET ISACTIVE = " + isActive.ToString() + @"
                       ,LOCAL_TD = " + localTd + @"
                       ,PARENT_ID = " + parentId + @"
                       ,FIRM_NAME = '" + firmName + @"'
                  WHERE FIRM_GUID = '" + myGuid + @"'
                end  ";


            ///*       -- update


            //    if ( select count(*) ADET from HP_FIRMS
            //         where ID > 0 
            //         and FIRM_GUID = '" + myGuid + @"'
            //         and ( ISACTIVE <> " + isActive.ToString() + @"
            //           or  LOCAL_TD <> " + localTd + @" 
            //           or  PARENT_ID <> " + parentId + @"
            //           or  FIRM_NAME <> '" + firmName + @"' 
            //    )) = 1
            //    begin
            //      UPDATE HP_FIRMS
            //        SET ISACTIVE = " + isActive.ToString() + @"
            //           ,LOCAL_TD = " + localTd + @"
            //           ,PARENT_ID = " + parentId + @"
            //           ,FIRM_NAME = '" + firmName + @"'
            //      WHERE FIRM_GUID = '" + myGuid + @"'
            //    end  ";
            //*/



            return s;

            #region
            /*
            begin transaction 
              if ( Select count(*) ADET from [HP_FIRM] where 0 = 0 
                 and [FIRM_GUID] = null 
                 ) = 0 
                 begin 
                 insert into [HP_FIRM] ( 
                       ISACTIVE, 
                       REC_DATE, 
                       FIRM_GUID, 
                       LOCAL_TD, 
                       LOCAL_ID, 
                       PARENT_ID, 
                       FIRM_NAME, 
                       HP_CODE, 
                       HP_NAME, 
                       HP_LONG_NAME1, 
                       HP_LONG_NAME2, 
                       OLD_ID ) values 
                 ( 0,   null,   null,   0,   0,   0,   null,   null,   null,   null,   null,   0 ) 
                  select MAX(ID) as ID from HP_FIRM 
                 end else 
                   Select 0 as ID 
            commit transaction 
            */
            #endregion

        }

        private string Sql_HP_UPDATES_Insert(
              string updateDate
            , string exeName
            , Int16 updateTd
            , int updateNo
            , string versionNo
            , string packetName
            , string about
            )
        {
            string s = "";

            if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                s = @"
                if ( Select count(*) ADET from [HP_UPDATES] where 0 = 0 
                     and [COMP_KEY] = '" + v.tComputer.Network_MACAddress + @"' 
                     and [UPDATE_TD] = " + updateTd + @"
                     and [UPDATE_NO] = " + updateNo + @"
                ) = 0 
                begin 
                  insert into [dbo].[HP_UPDATES]
                   ([COMP_KEY]
                   ,[ISACTIVE]
                   ,[REC_DATE]
                   ,[UPDATE_DATE]
                   ,[EXE_NAME]
                   ,[UPDATE_TD]
                   ,[UPDATE_NO]
                   ,[VERSION_NO]
                   ,[PACKET_NAME]
                   ,[ABOUT]) values ( 
                        '" + v.tComputer.Network_MACAddress + @"'
                      , 0 -- ISACTIVE 
                      , getdate() 
                      , " + Tarih_Formati(Convert.ToDateTime(updateDate)) + @"
                      , '" + exeName + @"'
                      , " + updateTd + @"
                      , " + updateNo + @"
                      , '" + versionNo + @"'
                      , '" + packetName + @"'
                      , '" + about + @"' ) 
                end 

                Select top 1 * from [HP_UPDATES] 
                where ISACTIVE = 0 
                and [COMP_KEY] = '" + v.tComputer.Network_MACAddress + @"' 
                and [UPDATE_TD] = " + updateTd + @"
                and [UPDATE_NO] = " + updateNo + @" ";

            return s;
        }

        public void SYS_Glyph_Read()
        {
            tSQLs sql = new tSQLs();

            string Sql = sql.SQL_MS_GLYPH_LIST();

            if (IsNotNull(Sql))
                SQL_Read_Execute(v.dBaseNo.Manager, v.ds_Icons, ref Sql, "MS_GLYPH", "");
        }

        public void SYS_IL_Read()
        {
            tSQLs sql = new tSQLs();

            string Sql = sql.SQL_IL_LIST();

            if (IsNotNull(Sql))
                SQL_Read_Execute(v.dBaseNo.Project, v.ds_ILList, ref Sql, "ILList", "");
        }
        public void SYS_Ilce_Read()
        {
            tSQLs sql = new tSQLs();

            string Sql = sql.SQL_ILCE_LIST();

            if (IsNotNull(Sql))
                SQL_Read_Execute(v.dBaseNo.Project, v.ds_IlceList, ref Sql, "ILCEList", "");
        }

        public void CrsTakvimiAyarla()
        {
            vTable vt = new vTable();
            vt.DBaseNo = v.dBaseNo.Project;
            vt.SchemasCode = "dbo";
            vt.TableName = "CrsTakvim";
            vt.ParentTable = "";
            vt.SqlScript = "";

            DataSet ds = new DataSet();

            string Sql = " Execute [dbo].[prc_CrsTakvimiAyarla] @FirmId = 0 ";

            Sql_ExecuteNon(ds, ref Sql, vt);

            ds.Dispose();
        }

        // LKP_ olan fieldler ve onlara ait listeler
        public void SYS_Types_List(DataSet ds, string List_Name)
        {
            tSQLs sql = new tSQLs();

            string Sql = sql.SQL_Types_List(List_Name);

            if (IsNotNull(Sql))
            {
                SQL_Read_Execute(v.dBaseNo.Manager, ds, ref Sql, "SYS_TYPES_" + List_Name, "");

                if (IsNotNull(ds))
                {
                    string s = "";

                    s = ds.Tables[0].Rows[0]["VALUE_INT"].ToString();

                    // Eğer -9 ise Data tablolarından okunması gerekiyor
                    if (s == "-9")
                    {
                        Sql = SQL_Data_Types(ds, List_Name);
                        SQL_Read_Execute(v.dBaseNo.Project, ds, ref Sql, "LKP_TABLE_" + List_Name, "");
                    }
                }

            }

        }

        private string SQL_Data_Types(DataSet ds, string Type_Name) // İPTAL
        {
            // İPTAL
            string s = string.Empty;

            /*
            tToolBox t = new tToolBox();

            
            string val_type_ = t.Set(ds.Tables[0].Rows[0]["VALUE_TYPE"].ToString(), "", "");
            string str_fname = t.Set(ds.Tables[0].Rows[0]["VALUE_STR"].ToString(), "", "null");
            string int_fname = t.Set(ds.Tables[0].Rows[0]["VALUE_INT_FNAME"].ToString(), "", "null");
            string cap_fname = t.Set(ds.Tables[0].Rows[0]["VALUE_CAPTION"].ToString(), "", "");
            string where = t.Set(ds.Tables[0].Rows[0]["WHERE_SQL"].ToString(), "", "");
            if (t.IsNotNull(where)) where = " where " + where;
            string tableName = Type_Name;

            s = " select "
            + "   " + val_type_ + "  VALUE_TYPE "
            + " , " + int_fname + "  VALUE_INT "
            + " , " + str_fname + "  VALUE_STR "
            + " , " + cap_fname + "  VALUE_CAPTION "
            + " from " + tableName + " a "
            + where 
            ;

            //+ " select "
            //+ "   VALUE_TYPE "
            //+ " , -9  VALUE_INT "
            //+ " , STR_FNAME      VALUE_STR "
            //+ " , CAPTION_FNAME  VALUE_CAPTION "
            //+ " , INT_FNAME      VALUE_INT_FNAME "
            //+ " from SYS_TYPES_T a "
            //+ " where a.TABLE_NAME = @type_name "
            */
            return s;
        }

        #endregion SYS_Types_List

        #region Computer MOS

        public void Find_MOS_MacAddr()
        {
            String macAddr = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

            //MessageBox.Show(macAddr.ToString());

            /*
            v.SQL = "";
            string id = "";
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_NetworkAdapter");
            foreach (ManagementObject obj in MOS.Get())
            {
                foreach (PropertyData data in obj.Properties)
                {
                    if ((data.Value != null) && 
                        ((data.Name == "MACAddress") |
                         (data.Name == "InterfaceIndex") |
                         (data.Name == "Caption")
                        ))
                    {
                        id = id + v.ENTER + "Name : " + data.Name + "    Value : " + data.Value.ToString();
                        v.SQL = v.SQL + v.ENTER + "Name : " + data.Name.PadLeft(26) + " : " + data.Value.ToString();
                    }
                }
            }
            MOS.Dispose();

            MessageBox.Show(id);
            */
        }

        public void Find_MOS(string hwclass, string propertName)
        {

            string id = "";

            v.SQL = "";

            ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\CIMV2", "Select * From " + hwclass);//Win32_BaseBoard");
            foreach (ManagementObject obj in MOS.Get())
            {
                id = id + v.ENTER + obj.ToString();
                //Convert.ToString(obj[propertName]);

                foreach (PropertyData data in obj.Properties)
                {
                    if (data.Value != null)
                    {
                        id = id + v.ENTER + "Name : " + data.Name + "    Value : " + data.Value.ToString();
                        v.SQL = v.SQL + v.ENTER + "Name : " + data.Name.PadLeft(26) + " : " + data.Value.ToString();
                    }
                }

            }
            MOS.Dispose();

            /*
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up

                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
            */


            /// https://msdn.microsoft.com/en-us/library/aa389273(v=vs.85).aspx

            ///Win32_Processor
            ///
            ///string Name 
            ///string ProcessorId
            ///string SystemName
            ///
            ///Win32_DiskDrive
            ///
            ///string   SerialNumber : 5VEKCDC2
            ///string   Model        : ST9500325AS
            ///string   Name         : \\.\PHYSICALDRIVE0

        }

        public async Task Get_ComputerAbout()
        {

            ///Win32_Processor
            ///
            ///string Name 
            ///string ProcessorId
            ///string SystemName
            ///
            ///Win32_DiskDrive
            ///
            ///string   SerialNumber;
            ///string   SystemName;

            /// Win32_ComputerSystem
            /// Win32_Processor
            /// Win32_DiskDrive
            /// Win32_PhysicalMemory 
            /// Win32_OperatingSystem
            /// 

            //try    pc Name için gerek yok
            //{
            //    ManagementObjectSearcher computerSystemSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            //    foreach (ManagementObject computerSystem in computerSystemSearcher.Get())
            //    {
            //        //Console.WriteLine($"PC Name: {computerSystem["Name"]}");
            //        //Console.WriteLine($"Service Tag: {computerSystem["OEMStringArray"][1]}");
            //        string pcName = computerSystem["Name"].ToString();
            //        //var serviceTag = computerSystem["OEMStringArray"][1];
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //throw;
            //}

            try
            {
                //ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_Processor ");
                ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject processor in processorSearcher.Get())
                {
                    v.tComputer.PcName = Convert.ToString(processor["SystemName"]);
                    v.tComputer.Processor_Name = Convert.ToString(processor["Name"]);
                    v.tComputer.Processor_Id = Convert.ToString(processor["ProcessorId"]);
                    v.tComputer.CPUType = processor["Name"].ToString();
                    v.tComputer.CPUSpeed = processor["MaxClockSpeed"].ToString() + " MHz";
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("ManagementObjectSearcher (Win32_Processor) :" + ex.Message);
                //throw;
            }
            try
            {
                ManagementObjectSearcher operSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject oper in operSearcher.Get())
                {
                    string oprSystem = oper["Name"].ToString();
                    v.tComputer.OperatingSystem = oprSystem.Substring(0, oprSystem.IndexOf("|"));
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("ManagementObjectSearcher (Win32_OperatingSystem) :" + ex.Message);
                //throw;
            }
            try
            {
                //ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_DiskDrive ");
                ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * From Win32_DiskDrive ");

                foreach (ManagementObject obj in MOS.Get())
                {
                    v.tComputer.DiskDrive_Name = Convert.ToString(obj["Name"]);
                    v.tComputer.DiskDrive_Model = Convert.ToString(obj["Model"]);
                    v.tComputer.DiskDrive_SerialNumber = Convert.ToString(obj["SerialNumber"]);
                    break; // birinci diski oku çık
                }

                MOS.Dispose();
            }
            catch (Exception)
            {
                //MessageBox.Show("ManagementObjectSearcher (Win32_DiskDrive) :" + e2.Message);
                //throw;
            }
        }

        public async Task Get_MacAddress()
        {
            /// Read computer network ethernet mac address
            /// 
            try
            {
                String macAddr = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

                v.tComputer.Network_MACAddress = macAddr;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Get MacAddress " + v.ENTER2 + ex.Message);
                //throw;
            }
        }

        #endregion Computer MOS

        #region  Create_PropertiesEdit_Model_JSON
        public string Create_PropertiesEdit_Model_JSON(string TableName, string Main_FieldName)
        {
            string value = string.Empty;

            #region PROP_SUBVIEW
            if (Main_FieldName == "PROP_SUBVIEW")
            {
                PROP_SUBVIEW obj = new PROP_SUBVIEW();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "SV_LIST")
            {
                SV_LIST obj = new SV_LIST();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_JOINTABLE
            if (Main_FieldName == "PROP_JOINTABLE")
            {
                PROP_JOINTABLE obj = new PROP_JOINTABLE();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "J_TABLE")
            {
                J_TABLE obj = new J_TABLE();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "J_WHERE")
            {
                J_WHERE obj = new J_WHERE();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "J_STN_FIELDS")
            {
                J_STN_FIELDS obj = new J_STN_FIELDS();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "J_CASE_FIELDS")
            {
                J_CASE_FIELDS obj = new J_CASE_FIELDS();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_VIEWS - MS_TABLES_IP
            if ((Main_FieldName == "PROP_VIEWS") && (TableName == "MS_TABLES_IP"))
            {
                PROP_VIEWS_IP obj = new PROP_VIEWS_IP();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "ALLPROP")
            {
                ALLPROP obj = new ALLPROP();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "GRID")
            {
                GRID obj = new GRID();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "TILE")
            {
                TILE obj = new TILE();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_VIEWS - MS_ITEMS
            if ((Main_FieldName == "PROP_VIEWS") && (TableName == "MS_ITEMS"))
            {
                PROP_VIEWS_ITEMS obj = new PROP_VIEWS_ITEMS();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "NAVBAR")
            {
                NAVBAR obj = new NAVBAR();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "NAVIGATIONPANE")
            {
                NAVIGATIONPANE obj = new NAVIGATIONPANE();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "ACCORDION")
            {
                ACCORDION obj = new ACCORDION();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_VIEWS - MS_LAYOUT - MS_GROUPS
            if ((Main_FieldName == "PROP_VIEWS") && (TableName == "MS_LAYOUT"))
            {
                PROP_VIEWS_LAYOUT obj = new PROP_VIEWS_LAYOUT();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if ((Main_FieldName == "PROP_VIEWS") && (TableName == "MS_GROUPS"))
            {
                PROP_VIEWS_GROUPS obj = new PROP_VIEWS_GROUPS();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if ((Main_FieldName == "PROP_VIEWS") && (TableName == "MS_LAYOUT"))
            {
                PROP_VIEWS_LAYOUT obj = new PROP_VIEWS_LAYOUT();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "DOCKPANEL")
            {
                DOCKPANEL obj = new DOCKPANEL();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "SPLIT")
            {
                SPLIT obj = new SPLIT();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "EDITPANEL")
            {
                EDITPANEL obj = new EDITPANEL();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "TLP")
            {
                TLP obj = new TLP();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "TLP_COLUMNS")
            {
                TLP_COLUMNS obj = new TLP_COLUMNS();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "TLP_ROWS")
            {
                TLP_ROWS obj = new TLP_ROWS();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_RUNTIME
            if (Main_FieldName == "PROP_RUNTIME")
            {
                PROP_RUNTIME obj = new PROP_RUNTIME();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "DRAGDROP")
            {
                DRAGDROP obj = new DRAGDROP();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "PRL_KRT")
            {
                PRL_KRT obj = new PRL_KRT();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "AUTO_LST")
            {
                AUTO_LST obj = new AUTO_LST();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "TABLEIPCODE_LIST2")
            {
                TABLEIPCODE_LIST2 obj = new TABLEIPCODE_LIST2();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }

            #endregion

            #region PROP_NAVIGATOR and 2
            // neden iki tane hatırlmadım ? 
            //if (Main_FieldName == "PROP_NAVIGATOR")
            //{
            //    PROP_NAVIGATOR obj = new PROP_NAVIGATOR();
            //    value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            //}
            if (Main_FieldName == "PROP_NAVIGATOR")
            {
                PROP_NAVIGATOR obj = new PROP_NAVIGATOR();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "TABLEIPCODE_LIST")
            {
                TABLEIPCODE_LIST obj = new TABLEIPCODE_LIST();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_SEARCH
            if (Main_FieldName == "PROP_SEARCH")
            {
                PROP_SEARCH obj = new PROP_SEARCH();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            if (Main_FieldName == "GET_FIELD_LIST")
            {
                GET_FIELD_LIST obj = new GET_FIELD_LIST();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            #region PROP_EXPRESSION
            if (Main_FieldName == "PROP_EXPRESSION")
            {
                PROP_EXPRESSION obj = new PROP_EXPRESSION();
                value = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            #endregion

            ////var definition = new { Name = "" };
            //PROP_SUBVIEW definition = new PROP_SUBVIEW();
            //string json1 = thisValue;
            //json1 = json1.Replace((char)34, (char)39);
            //var customer1 = JsonConvert.DeserializeAnonymousType(json1, definition);   


            /// {  "CAPTION": null,  "SV_VALUE": null,  "TABLEIPCODE": null }
            /// şeklinde gelen yapıyı
            /// {  "CAPTION": "null",  "SV_VALUE": "null",  "TABLEIPCODE": "null" }
            /// şekline çeviriyoruz

            string s = ": " + (char)34 + "null" + (char)34;
            Str_Replace(ref value, ": null", s);

            /// sonucu gönder
            return value;
        }
        #endregion  Create_PropertiesEdit_Model_JSON

        #region tCheckedValue
        public bool tWorkingCheck(Form tForm, TABLEIPCODE_LIST item, DataRow sourceDataRow)
        {
            bool onay1 = true;
            bool onay2 = true;

            string read_value = string.Empty;

            // FIRST = SOURCE
            string chc_IPCode = "";
            string chc_FName = "";
            string chc_Value = "";
            string chc_Operand = "";

            // FIRST = SOURCE
            if (item.CHC_IPCODE3 != null)
                chc_IPCode = item.CHC_IPCODE3.ToString();
            if (item.CHC_FNAME3 != null)
                chc_FName = item.CHC_FNAME3.ToString();
            if (item.CHC_VALUE3 != null)
                chc_Value = item.CHC_VALUE3.ToString();
            if (item.CHC_OPERAND3 != null)
                chc_Operand = item.CHC_OPERAND3.ToString();

            if (IsNotNull(chc_IPCode) &&
                IsNotNull(chc_FName) &&
                IsNotNull(chc_Value) &&
                IsNotNull(chc_Operand))
            {
                /// arama moturndan geliyor
                /// DataRow sourceDataRow


                // value ile bize tek bir değer döner 
                // bu nedenle chc_Value içinde sorgudan bize dönen value değeri varmı diye kontrol edilir
                if (sourceDataRow == null)
                    read_value = TableFieldValueGet(tForm, chc_IPCode, chc_FName);

                if (sourceDataRow != null)
                {
                    try
                    {
                        read_value = sourceDataRow[chc_FName].ToString();
                        if (read_value == "True") read_value = "1";
                        if (read_value == "False") read_value = "0";
                    }
                    catch (Exception)
                    {
                        read_value = "";
                        //
                    }
                }

                //eğer boşluksa zaten herhangi bir işlem veri girii yok demektir
                if ((read_value == "") && (chc_Value.ToUpper() != "NULL"))
                    read_value = "0";
                //if ((read_value == "") || (chc_Value.ToUpper() == "NULL"))
                //    read_value = "NULL";

                // eğer bir daha boş değer gelir ise
                //if (t.IsNotNull(read_value) == false) read_value = "0";

                if (read_value != "")
                {
                    if ((chc_Value.IndexOf(read_value) > -1) &&
                        (IsNotNull(chc_Operand) == false))
                    {
                        // eğer buraya kadar geldiyse 
                        // öndeki chc_xxxx kontrollerinden geçti,  
                        // yani onayı hak etti demektir
                        onay1 = true;
                    }

                    if (IsNotNull(chc_Operand))
                    {
                        onay1 = myOperandControl(read_value, chc_Value, chc_Operand);
                    }
                }
            }


            return ((onay1) && (onay2));
        }

        public bool tWorkingCheck(Form tForm, PROP_NAVIGATOR prop_item, string mst_TableIPCode)
        {
            //tToolBox t = new tToolBox();

            // formun açılması için default onay ver 
            // chc_Value     şartı var ise ona göre yeniden değerlendir = form_open1
            // chc_Value_SEC şartı var ise ona göre yeniden değerlendir = form_open2
            bool form_open1 = true;
            bool form_open2 = true;

            string read_value = string.Empty;

            // FIRST = SOURCE
            string chc_IPCode = prop_item.CHC_IPCODE.ToString();
            string chc_FName = prop_item.CHC_FNAME.ToString();
            string chc_Value = prop_item.CHC_VALUE.ToString();
            string chc_Operand = prop_item.CHC_OPERAND.ToString();

            /// bu kısım sonradan eklendiği için gerekli oldu
            /// önceki prop_item larda null geliyor
            if (prop_item.CHC_IPCODE_SEC == null) prop_item.CHC_IPCODE_SEC = "";
            if (prop_item.CHC_FNAME_SEC == null) prop_item.CHC_FNAME_SEC = "";
            if (prop_item.CHC_VALUE_SEC == null) prop_item.CHC_VALUE_SEC = "";
            if (prop_item.CHC_OPERAND_SEC == null) prop_item.CHC_OPERAND_SEC = "";

            // SEC = SECOND or TARGET
            string chc_IPCode_SEC = prop_item.CHC_IPCODE_SEC.ToString();
            string chc_FName_SEC = prop_item.CHC_FNAME_SEC.ToString();
            string chc_Value_SEC = prop_item.CHC_VALUE_SEC.ToString();
            string chc_Operand_SEC = prop_item.CHC_OPERAND_SEC.ToString();

            /// bu IPCode ler dragdrop sırasında oluşuyor

            if (((chc_IPCode == "FIRST") || (chc_IPCode == "SOURCE")) &&
                IsNotNull(v.con_DragDropSourceTableIPCode))
                chc_IPCode = v.con_DragDropSourceTableIPCode;

            if (((chc_IPCode_SEC == "SECOND") || (chc_IPCode_SEC == "TARGET")) &&
                IsNotNull(v.con_DragDropTargetTableIPCode))
                chc_IPCode_SEC = v.con_DragDropTargetTableIPCode;


            #region Check işlemleri / First veya Soruce için şart varsa
            if (IsNotNull(chc_IPCode) &&
                IsNotNull(chc_FName) &&
                IsNotNull(chc_Value))
            {
                // chc_xxxx ler ile bir fieldin value sine bakarak istedğimiz formu açabiliriz

                // yani  CARI_TIPI = 5 veya CARI_TIPI = 6 veya CARI_TIPI = 10 için  XXXXform unu aç

                // chc_FName  = CARI_TIPI   <<< 
                // chc_Value  = 5, 6, 10    <<< şeklinde olabilir 

                // value ile bize tek bir değer döner 
                // bu nedenle chc_Value içinde sorgudan bize dönen value değeri varmı diye kontrol edilir
                read_value = TableFieldValueGet(tForm, chc_IPCode, chc_FName);

                #region // eğer boş ise
                if (IsNotNull(read_value) == false)
                {
                    //tTabPage_FNLNVOL_FNLNVOL_BNL0123
                    //FNLNVOL.FNLNVOL_BNL01|23   grid üzerindeki IPCode bu olunca chc_IPCode ile bulunamıyor

                    /// chc_IPCode      = FNLNVOL.FNLNVOL_BNL01   
                    /// mst_TableIPCode = FNLNVOL.FNLNVOL_BNL01|23

                    if (mst_TableIPCode.LastIndexOf(chc_IPCode) > -1)
                    {
                        read_value = TableFieldValueGet(tForm, mst_TableIPCode, chc_FName);


                        if (IsNotNull(read_value))
                        {
                            /// RTABLEIPCODE:FNLNVOL.FNLNVOL_BNL01
                            /// RTABLEIPCODE:FNLNVOL.FNLNVOL_BNL01|23

                            //t.Str_Replace(ref block, "RTABLEIPCODE:" + chc_IPCode, "RTABLEIPCODE:" + mst_TableIPCode);

                            foreach (var item in prop_item.TABLEIPCODE_LIST)
                            {
                                if (item.RTABLEIPCODE.ToString() == chc_IPCode)
                                    item.RTABLEIPCODE = mst_TableIPCode;
                            }
                        }

                        //eğer boşluksa zaten herhangi bir işlem veri girii yok demektir
                        if ((read_value == "") && (chc_Value.ToUpper() != "NULL"))
                            read_value = "0";

                    }

                    if ((read_value == "") || (chc_Value.ToUpper() == "NULL"))
                        read_value = "NULL";
                }
                #endregion boş ise

                // eğer bir daha boş değer gelir ise
                //if (t.IsNotNull(read_value) == false) read_value = "0";

                if (read_value != "")
                {
                    if ((chc_Value.IndexOf(read_value) > -1) &&
                        (IsNotNull(chc_Operand) == false))
                    {
                        // eğer buraya kadar geldiyse 
                        // öndeki chc_xxxx kontrollerinden geçti,  
                        // yani onayı hak etti demektir
                        form_open1 = true;
                    }

                    if (IsNotNull(chc_Operand))
                    {
                        form_open1 = myOperandControl(read_value, chc_Value, chc_Operand);
                    }
                }
            }
            #endregion Check işlemleri1

            #region ELSE kontrolü
            if ((chc_Value.IndexOf("ELSE") > -1))
            {
                // önceki kontrollere henüz yakalanmamış
                // ve else koşuluna sıra gelmişse
                form_open1 = true;
            }
            #endregion 

            #region Check işlemleri / Second veya Target için şart varsa
            if (IsNotNull(chc_IPCode_SEC) &&
                IsNotNull(chc_FName_SEC) &&
                IsNotNull(chc_Value_SEC))
            {
                read_value = TableFieldValueGet(tForm, chc_IPCode_SEC, chc_FName_SEC);

                if ((read_value == "") || (chc_Value_SEC.ToUpper() == "NULL"))
                    read_value = "NULL";

                if (read_value != "")
                {
                    if ((chc_Value_SEC.IndexOf(read_value) > -1) &&
                        (IsNotNull(chc_Operand_SEC) == false))
                    {
                        // eğer buraya kadar geldiyse 
                        // öndeki chc_xxxx kontrollerinden geçti,  
                        // yani onayı hak etti demektir
                        form_open2 = true;
                    }

                    if (IsNotNull(chc_Operand_SEC))
                    {
                        form_open2 = myOperandControl(read_value, chc_Value_SEC, chc_Operand_SEC);
                    }
                }

            }
            #endregion Check işlemleri2

            return ((form_open1) && (form_open2));
        }

        public string tCheckedValue(DataRow row, string fieldName, string value)
        {
            DataSet ds = row.Table.DataSet;
            return tCheckedValue(ds, fieldName, value);
        }

        public bool IsIntFieldType(DataSet ds, string fieldName)
        {
            bool onay = false;
            string displayFormat = string.Empty;

            int ftype = Find_Field_Type_Id(ds, fieldName, ref displayFormat);

            //* rakam  56, 48, 127, 52, 60, 62, 59, 106, 108
            if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 106) | (ftype == 108))
            {
                onay = true;
            }

            return onay;
        }

        public string tCheckedValue(DataSet ds, string fieldName, string value)
        {
            string displayFormat = string.Empty;
            int ftype = Find_Field_Type_Id(ds, fieldName, ref displayFormat);

            //* rakam  56, 48, 127, 52, 60, 62, 59, 106, 108
            if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 106) | (ftype == 108))
            {
                // Eğer veri (Rakam) yok ise Sıfır bas
                if (value == "") value = "0";
            }
            // datetime türü 40
            if ((ftype == 40) || (ftype == 58) || (ftype == 61))
            {
                if (value == "") value = "0";
            }
            //* time türü 41
            if (ftype == 41)
            {
                if (value == "") value = "00:00";
            }
            //* bit türü 104
            if (ftype == 104)
            {
                // burası saçma oldu
                // DataCopy de patlak verdi
                if ((value == "False") || (value == ""))
                {
                    value = "False"; // "0"  bunu anlamadım bir sıfır istiyor bir False
                }
                else if (value == "True")
                {
                    value = "True"; // "1";
                }
            }

            return value;
        }
        #endregion tCheckedValue

        public string getDateTimeString(DateTime dt)
        {
            string yil = dt.Year.ToString();
            string ay = dt.Month.ToString();
            string gun = dt.Day.ToString();
            string saat = dt.Hour.ToString();
            string dakk = dt.Minute.ToString();

            if (ay.Length == 1) ay = "0" + ay;
            if (gun.Length == 1) gun = "0" + gun;
            if (saat.Length == 1) saat = "0" + saat;
            if (dakk.Length == 1) dakk = "0" + dakk;

            return yil + ay + gun + "_" + saat + dakk;
        }

        #endregion Diğer

        #region *Find Functions

        public string Find_ListAvailableMSSQLServers()
        {
            //v.IsWaitOpen = false;
            //WaitFormClose();
            Application.DoEvents();

            //AlertMessage("MSSQL Server", "Network üzerindeki MSSQL Server tespit ediliyor...");
            //WaitFormOpen(null, "Network üzerindeki MSSQL Server tespit ediliyor...");
            //Application.DoEvents();

            MessageBox.Show("Network üzerindeki MSSQL Server tespit edilecek, lütfen biraz bekleyin ...", "MSSQL Server");

            // Örnekleyici al
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            // Tabloyu al
            DataTable table = instance.GetDataSources();
            // Tabloyu istediğiniz şekilde kullanın
            string serverName = "";
            string instanceName = "";
            foreach (DataRow row in table.Rows)
            {
                serverName = row["ServerName"].ToString();
                instanceName = row["InstanceName"].ToString();
                break;
                //Console.WriteLine("{0}\\{1}", row["ServerName"], row["InstanceName"]);
            }

            //v.IsWaitOpen = false;
            //WaitFormClose();
            //Application.DoEvents();

            // test için kapatmayı unutma
            //serverName = "LAPTOP-ACER1";
            //instanceName = "SQLEXPRESS";

            MessageBox.Show(serverName + "\\" + instanceName, "Tespit edilen MSSQL Server");
            return serverName + "\\"+ instanceName;
        }

        #region Form

        public Form Find_Form(string FormCode)
        {
            // Eğer formu formcode ile bulamazsan ismiylede bulabilirsin
            //Form tForm = Application.OpenForms["tForm_" + Name];

            Form tForm = null;

            int t = Application.OpenForms.Count;

            for (int i = 0; i < t; i++)
            {
                if (Application.OpenForms[i].AccessibleDescription == FormCode)
                {
                    return Application.OpenForms[i];
                }
            }

            return tForm;
        }

        public Form Find_Form(object sender)
        {
            string function_name = "Find_Form(object)";
            Takipci(function_name, "", '{');

            // Eğer formu object ile bulamazsan ismiylede bulabilirsin
            //Form tForm = Application.OpenForms["tForm_" + Name];

            string MyObjectName = sender.ToString();

            Form tForm = null;

            if (MyObjectName.IndexOf("DevExpress.XtraWizard.WizardControl") > -1)
            {
                tForm = ((DevExpress.XtraWizard.WizardControl)sender).FindForm();
                return tForm;
            }
            if (MyObjectName.IndexOf("DevExpress.XtraBars.BarManager") > -1)
            {
                tForm = ((DevExpress.XtraBars.BarManager)sender).Form.FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraBars.Ribbon.RibbonBarManager")
            {
                tForm = ((DevExpress.XtraBars.Ribbon.RibbonBarManager)sender).Form.FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraBars.BarButtonItem")
            {
                tForm = (((DevExpress.XtraBars.BarButtonItem)sender).Manager).Form.FindForm();
                //tForm = ((DevExpress.XtraBars.BarButtonItem)sender).Manager.Form.FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraBars.BarLargeButtonItem")
            {
                tForm = (((DevExpress.XtraBars.BarLargeButtonItem)sender).Manager).Form.FindForm();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraEditors.SpinEdit")
            {
                tForm = ((DevExpress.XtraEditors.SpinEdit)sender).FindForm();
                return tForm;
            }


            if (MyObjectName == "DevExpress.XtraEditors.ImageComboBoxEdit")
            {
                tForm = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.CheckEdit")
            {
                tForm = ((DevExpress.XtraEditors.CheckEdit)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.DateEdit")
            {
                tForm = ((DevExpress.XtraEditors.DateEdit)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.SimpleButton")
            {
                tForm = ((DevExpress.XtraEditors.SimpleButton)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.ButtonEdit")
            {
                tForm = ((DevExpress.XtraEditors.ButtonEdit)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.TextEdit")
            {
                tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.DataNavigator")
            {
                tForm = ((DevExpress.XtraEditors.DataNavigator)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraGrid.GridControl")
            {
                tForm = ((GridControl)sender).FindForm();
                return tForm;
            }
            if (MyObjectName.IndexOf("DevExpress.XtraEditors.ZoomTrackBarControl") > -1)
            {
                tForm = ((DevExpress.XtraEditors.ZoomTrackBarControl)sender).FindForm();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraEditors.ListBoxControl")
            {
                tForm = ((DevExpress.XtraEditors.ListBoxControl)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.CheckButton")
            {
                tForm = ((DevExpress.XtraEditors.CheckButton)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.GroupControl")
            {
                tForm = ((DevExpress.XtraEditors.GroupControl)sender).FindForm();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraTreeList.TreeList")
            {
                tForm = ((DevExpress.XtraTreeList.TreeList)sender).FindForm();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView")
            {
                tForm = ((DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView)sender).GridControl.FindForm();
                return tForm;
            }

            //DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView

            if (MyObjectName == "DevExpress.XtraEditors.TileItem")
            {
                //    tForm = ((DevExpress.XtraEditors.TileItem)sender). // FindForm();
            }

            if (tForm == null)
            {
                MessageBox.Show("DİKKAT : " + MyObjectName + " ile tForm tespiti başarısız oldu ...", function_name);
            }

            Takipci(function_name, "", '}');

            return tForm;
        }

        public Form Find_TableIPCode_XtraEditors(object sender, ref string TableIPCode,
               ref string FieldName, ref string Value, ref string Tag)
        {
            Form tForm = null;

            string MyObjectName = sender.ToString();

            //tEdit.Name = "Column_" + tFieldName;
            //tEdit.Properties.AccessibleName = TableIPCode;

            if (MyObjectName == "DevExpress.XtraEditors.ImageComboBoxEdit")
            {
                tForm = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).FindForm();
                TableIPCode = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleName;
                FieldName = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Name.ToString();
                FieldName = FieldName.Substring(7, FieldName.Length - 7); // = "Column_" + tFieldName;
                //if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue != null)
                        Value = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue.ToString();
                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Tag != null)
                    Tag = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Tag.ToString();
                return tForm;
            }
            if (MyObjectName == "DevExpress.XtraEditors.CheckEdit")
            {
                tForm = ((DevExpress.XtraEditors.CheckEdit)sender).FindForm();
                TableIPCode = ((DevExpress.XtraEditors.CheckEdit)sender).Properties.AccessibleName;
                FieldName = ((DevExpress.XtraEditors.CheckEdit)sender).Name.ToString();
                FieldName = FieldName.Substring(7, FieldName.Length - 7); // = "Column_" + tFieldName;
                if (((DevExpress.XtraEditors.CheckEdit)sender).EditValue != null)
                    Value = ((DevExpress.XtraEditors.CheckEdit)sender).EditValue.ToString();
                if (((DevExpress.XtraEditors.CheckEdit)sender).Tag != null)
                    Tag = ((DevExpress.XtraEditors.CheckEdit)sender).Tag.ToString();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraEditors.CalcEdit")
            {
                tForm = ((DevExpress.XtraEditors.CalcEdit)sender).FindForm();
                TableIPCode = ((DevExpress.XtraEditors.CalcEdit)sender).Properties.AccessibleName;
                FieldName = ((DevExpress.XtraEditors.CalcEdit)sender).Name.ToString();
                FieldName = FieldName.Substring(7, FieldName.Length - 7);
                if (((DevExpress.XtraEditors.CalcEdit)sender).EditValue != null)
                    Value = ((DevExpress.XtraEditors.CalcEdit)sender).EditValue.ToString();
                if (((DevExpress.XtraEditors.CalcEdit)sender).Tag != null)
                    Tag = ((DevExpress.XtraEditors.CalcEdit)sender).Tag.ToString();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraEditors.TextEdit")
            {
                tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                TableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleName;
                FieldName = ((DevExpress.XtraEditors.TextEdit)sender).Name.ToString();
                FieldName = FieldName.Substring(7, FieldName.Length - 7); // = "Column_" + tFieldName;
                if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                    Value = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();
                if (((DevExpress.XtraEditors.TextEdit)sender).Properties.Tag != null)
                    Tag = ((DevExpress.XtraEditors.TextEdit)sender).Properties.Tag.ToString();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraEditors.DateEdit")
            {
                tForm = ((DevExpress.XtraEditors.DateEdit)sender).FindForm();
                TableIPCode = ((DevExpress.XtraEditors.DateEdit)sender).Properties.AccessibleName;
                FieldName = ((DevExpress.XtraEditors.DateEdit)sender).Name.ToString();
                FieldName = FieldName.Substring(7, FieldName.Length - 7); // = "Column_" + tFieldName;
                if (((DevExpress.XtraEditors.DateEdit)sender).EditValue != null)
                    Value = ((DevExpress.XtraEditors.DateEdit)sender).EditValue.ToString();
                if (((DevExpress.XtraEditors.DateEdit)sender).Properties.Tag != null)
                    Tag = ((DevExpress.XtraEditors.DateEdit)sender).Properties.Tag.ToString();
                return tForm;
            }

            if (MyObjectName == "DevExpress.XtraEditors.ToggleSwitch")
            {
                tForm = ((DevExpress.XtraEditors.ToggleSwitch)sender).FindForm();
                TableIPCode = ((DevExpress.XtraEditors.ToggleSwitch)sender).Properties.AccessibleName;
                FieldName = ((DevExpress.XtraEditors.ToggleSwitch)sender).Name.ToString();
                FieldName = FieldName.Substring(7, FieldName.Length - 7); // = "Column_" + tFieldName;
                if (((DevExpress.XtraEditors.ToggleSwitch)sender).EditValue != null)
                    Value = ((DevExpress.XtraEditors.ToggleSwitch)sender).EditValue.ToString();
                if (((DevExpress.XtraEditors.ToggleSwitch)sender).Tag != null)
                    Tag = ((DevExpress.XtraEditors.ToggleSwitch)sender).Tag.ToString();
                return tForm;
            }
            //((DevExpress.XtraEditors.CheckEdit)sender).Parent
            return tForm;
        }

        #endregion Form

        #region Control

        public string Find_TableIPCode(Control cntrl)
        {
            string code = string.Empty;

            if (cntrl != null)
                if (cntrl.AccessibleName != null)
                    code = cntrl.AccessibleName.ToString();

            return code;
        }

        public Control Find_Control(Form tForm, string controlName)
        {
            string[] cs = new string[] { };
            Control c = Find_Control(tForm, controlName, "", cs);
            return c;
        }

        public Control Find_Control(Form tForm, string Name, string AccessibleName, string[] ControlType)
        {
            if (tForm == null)
            {
                //MessageBox.Show("DİKKAT : Aranan nesne için önce -Form-un tespit edilmesi gerekir ( Form tanımsız, belirsiz ) ...", "Find_Control");
                return null;
            }

            if (Name == "tMyFormBox")
                Name = "tMyFormBox_" + tForm.Name.ToString();
            if (Name == "tMyMemo")
                Name = "tMyMemo_" + tForm.Name.ToString();
            if (Name == "tMyGroup")
                Name = "tMyGroup_" + tForm.Name.ToString();


            byte i = 0;
            int x = ControlType.Length;

            if ((Name != "") && (AccessibleName == "") && (x == 0)) i = 1;
            if ((Name == "") && (AccessibleName != "") && (x == 0)) i = 2;
            if ((Name == "") && (AccessibleName == "") && (x != 0)) i = 3;

            if ((Name != "") && (AccessibleName != "") && (x == 0)) i = 4;
            if ((Name == "") && (AccessibleName != "") && (x != 0)) i = 5;
            if ((Name != "") && (AccessibleName == "") && (x != 0)) i = 6;

            if ((Name != "") && (AccessibleName != "") && (x != 0)) i = 7;

            // FieldName göre control tespiti
            if (AccessibleName == "FIELDNAME") i = 8;

            // controlun TAG üzerindeki MASTEITEM_NO kontrol edilecek
            if (AccessibleName == "TAG") i = 9;

            #region Control1

            if (v.ControlListView)
            {
                v.ControlList = string.Empty;
                v.ControlList = v.ControlList + "[ *** " + tForm.Name.ToString() + " *** ]" + v.ENTER2;
            }
            
            Control fc = _Control(tForm.Controls, Name, AccessibleName, ControlType, i);
            
            #endregion // Control1

            if (v.ControlListView)
                v.SQL = v.ControlList + v.SQL;

            return fc; // null
        }

        private Control _Control(Control.ControlCollection controlCollection, string Name, string AccessibleName, string[] ControlType, int i)
        {
            Control ret = null;
            foreach (Control c in controlCollection)
            {
                if (c.Controls.Count > 0)
                {
                    ret = _Control(c.Controls, Name, AccessibleName, ControlType, i);
                    if (ret != null)
                        break;
                }
                //if (c.GetType().ToString() == "DevExpress.XtraEditors.SimpleButton")
                //{
                //    string stop_ = "beklebakalım";
                //    string stop2_ = "";
                //    string accessibleName_ = ((DevExpress.XtraEditors.SimpleButton)c).AccessibleName;

                //    if (accessibleName_ == AccessibleName && Name == "simpleButton_ek5")
                //    {
                //        if (c.Name == "simpleButton_ek5")
                //             stop2_ = "beklebakalım2";
                //    }
                //}
                if (Control_(c, Name, AccessibleName, ControlType, i))
                {
                    ret = c;
                    break;
                }
                if (c.GetType().ToString() == "DevExpress.XtraEditors.PopupContainerEdit")
                {
                    DevExpress.XtraEditors.PopupContainerControl popupContainerControl = ((DevExpress.XtraEditors.PopupContainerEdit)c).Properties.PopupControl;
                    //Control.ControlCollection popupControlCollection = popupContainerControl.Controls;
                    ret = _Control(popupContainerControl.Controls, Name, AccessibleName, ControlType, i);
                    if (ret != null)
                        break;
                }
            }
            return ret;
        }

        public void External_Controls_Enabled(Form tForm, DataSet dsData, Control cntrl)
        {
            /// ismi aynı olmayan yalnız aynı dataset i kullanan diğer cnrtl ise
            /// EXTERNAL_TABLE_IP_CODE kulanan cntrl leride enabled özelliğini değiştirmek için
            ///
            string myProp = dsData.Namespace.ToString();

            /// kendine bağlı IP yok ise geri dönsün
            /// 
            if (myProp.IndexOf("External_IP:True") == -1) return;

            string Name = cntrl.Name;
            // TableIPCode
            string Extrenal_TableIPCode = cntrl.AccessibleName;

            #region Control1
            foreach (Control c in tForm.Controls)
            {
                if (c.AccessibleDefaultActionDescription != null)
                {
                    if ((c.Name != Name) && (c.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                    {
                        c.Enabled = IsNotNull(dsData);
                    }
                }

                if (c.Controls.Count > 0)
                {
                    #region Control2
                    foreach (Control c2 in c.Controls)
                    {
                        if (c2.AccessibleDefaultActionDescription != null)
                        {
                            if ((c2.Name != Name) && (c2.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                            {
                                c2.Enabled = IsNotNull(dsData);
                            }
                        }

                        if (c2.Controls.Count > 0)
                        {
                            #region Control3
                            foreach (Control c3 in c2.Controls)
                            {
                                if (c3.AccessibleDefaultActionDescription != null)
                                {
                                    if ((c3.Name != Name) && (c3.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                    {
                                        c3.Enabled = IsNotNull(dsData);
                                    }
                                }

                                if (c3.Controls.Count > 0)
                                {
                                    #region Control4
                                    foreach (Control c4 in c3.Controls)
                                    {
                                        if (c4.AccessibleDefaultActionDescription != null)
                                        {
                                            if ((c4.Name != Name) && (c4.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                            {
                                                c4.Enabled = IsNotNull(dsData);
                                            }
                                        }

                                        if (c4.Controls.Count > 0)
                                        {
                                            #region Control5
                                            foreach (Control c5 in c4.Controls)
                                            {
                                                if (c5.AccessibleDefaultActionDescription != null)
                                                {
                                                    if ((c5.Name != Name) && (c5.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                                    {
                                                        c5.Enabled = IsNotNull(dsData);
                                                    }
                                                }

                                                if (c5.Controls.Count > 0)
                                                {
                                                    #region Control6
                                                    foreach (Control c6 in c5.Controls)
                                                    {
                                                        if (c6.AccessibleDefaultActionDescription != null)
                                                        {
                                                            if ((c6.Name != Name) && (c6.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                                            {
                                                                c6.Enabled = IsNotNull(dsData);
                                                            }
                                                        }

                                                        if (c6.Controls.Count > 0)
                                                        {
                                                            #region Control7
                                                            foreach (Control c7 in c6.Controls)
                                                            {
                                                                if (c7.AccessibleDefaultActionDescription != null)
                                                                {
                                                                    if ((c7.Name != Name) && (c7.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                                                    {
                                                                        c7.Enabled = IsNotNull(dsData);
                                                                    }
                                                                }

                                                                if (c7.Controls.Count > 0)
                                                                {
                                                                    #region Control8
                                                                    foreach (Control c8 in c7.Controls)
                                                                    {
                                                                        if (c8.AccessibleDefaultActionDescription != null)
                                                                        {
                                                                            if ((c8.Name != Name) && (c8.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                                                            {
                                                                                c8.Enabled = IsNotNull(dsData);
                                                                            }
                                                                        }

                                                                        if (c8.Controls.Count > 0)
                                                                        {
                                                                            #region Control9
                                                                            foreach (Control c9 in c8.Controls)
                                                                            {
                                                                                if (c9.AccessibleDefaultActionDescription != null)
                                                                                {
                                                                                    if ((c9.Name != Name) && (c9.AccessibleDefaultActionDescription == Extrenal_TableIPCode))
                                                                                    {
                                                                                        c9.Enabled = IsNotNull(dsData);
                                                                                    }
                                                                                }

                                                                                if (c9.Controls.Count > 0)
                                                                                {


                                                                                }

                                                                            }
                                                                            #endregion // Control9

                                                                        }

                                                                    }
                                                                    #endregion // Control8

                                                                }

                                                            }
                                                            #endregion // Control7

                                                        }

                                                    }
                                                    #endregion // Control6

                                                }

                                            }
                                            #endregion // Control5

                                        }

                                    }
                                    #endregion // Control4

                                }

                            }
                            #endregion // Control3
                        }

                    }
                    #endregion // Control2
                }
            }
            #endregion Control1
        }

        private Boolean Control_(Control c, string Name, string AccessibleName, string[] ControlType, int i)
        {
            Boolean sonuc = false;

            //if (Name.ToUpper() == "TEST")
            //    MessageBox.Show(c.Name + "//" + c.AccessibleName + "//" + c.ToString() + "//" + c.GetType());

            //if (v.Takip_Find == true)
            //    v.SQL = v.SQL + 
            //        (c.Name + "//" + c.AccessibleName + "//" + c.ToString() + "//" + c.GetType()+v.ENTER);

            if (v.ControlListView)
            {
                v.ControlList = v.ControlList + c.ToString() + " ( " + c.Name.ToString() + " | " + c.Text + " )" + v.ENTER;

                //if (c.Name.IndexOf("tDataNavigator_") > -1)
                //{
                //    v.Kullaniciya_Mesaj_Var = c.Name;  
                //}

                if (AccessibleName == "CR.CR_OMARA_L01")
                {
                    v.Kullaniciya_Mesaj_Var = AccessibleName;
                }

            }

            Boolean controls = false;
            if ((i == 3) || (i == 5) || (i == 6) || (i == 7))
            {
                int count = ControlType.Count();

                for (int t = 0; t < count; t++)
                {
                    if (c.ToString() == ControlType[t].ToString())
                    { controls = true; break; }
                }
            }

            if (i == 1)
                if (c.Name == Name) sonuc = true;
            if (i == 2) if (c.AccessibleName == AccessibleName) sonuc = true;
            if (i == 3) if (controls == true) sonuc = true;
            if (i == 4) 
                if ((c.Name == Name) && (c.AccessibleName == AccessibleName)) sonuc = true;
            if (i == 5)
                if ((c.AccessibleName == AccessibleName) && (controls == true)) sonuc = true;
            if (i == 6) if ((c.Name == Name) && (controls == true)) sonuc = true;
            if (i == 7) if ((c.Name == Name) && (c.AccessibleName == AccessibleName) && (controls == true)) sonuc = true;

            // c.AccessibleName = UST/OMS/BDekont.NakitIslemler_L01|1
            // AccessibleName   = UST/OMS/BDekont.NakitIslemler_L01
            // durumunu yakalamak için 
            // UST/OMS/BDekont.NakitIslemler_L01|1 : bu durum AcordionDinamik içinde oluşuyor
            
            if ((controls == true) && (sonuc == false) && IsNotNull(c.AccessibleName))
            {
                if (c.AccessibleName.IndexOf("|") > -1)
                    if (c.AccessibleName.IndexOf(AccessibleName) > -1) sonuc = true;
            }

            // Controlun FieldName bakarak control tespiti, 
            // FieldName de controlun Name ne veriliyor   
            // tEdit.Name = "Column_" + tfieldname;    şeklinde
            if (i == 8)
            {
                if ((c.Name.IndexOf(Name) > -1) &&
                    (c.Name.IndexOf("Panel_") == -1))
                    sonuc = true;
                // MessageBox.Show(c.Name.IndexOf(Name).ToString());
            }

            // Controlun Tag üzerindeki MASTER_ITEM_NO kontrol edilecek
            if (i == 9)
                if (c.Tag != null)
                    if (c.Tag.ToString() == Name) sonuc = true;

            return sonuc;
        }

        public Control Find_Control_Tag(Form tForm, string Tag)
        {
            string[] ControlType = { };

            return Find_Control(tForm, Tag, "TAG", ControlType);
        }

        public Control findControlMenu(Form tForm, string menuType, string menuName)
        {
            Control cntrl = new Control();

            if (menuType == "")
                menuType = "DevExpress.XtraEditors.TileControl";

            // Öncelik
            string[] controls = new string[] { menuType };
            cntrl = Find_Control(tForm, menuName, "", controls);
            if (cntrl != null)
            {
                return cntrl;
            }
            return null;
        }

        public Control findControlMenu(Form tForm)
        {
            Control cntrl = new Control();

            string[] controls = new string[] { "DevExpress.XtraBars.Navigation.TileNavPane",
                                               "DevExpress.XtraBars.Ribbon.RibbonControl",
                                               "DevExpress.XtraToolbox.ToolboxControl"
                                             };

            cntrl = Find_Control(tForm, "", "", controls);
            if (cntrl != null)
            {
                return cntrl;
            }
            return null;
        }

        public Control Find_Control_View(Form tForm, string TableIPCode)
        {

            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraGrid.GridControl",
                                               "DevExpress.XtraVerticalGrid.VGridControl",
                                               "DevExpress.XtraDataLayout.DataLayoutControl",
                                               "DevExpress.XtraTreeList.TreeList",
                                               "DevExpress.XtraGrid.Views.Tile.TileView",
                                               "DevExpress.XtraScheduler.SchedulerControl"
                                               //"DevExpress.XtraEditors.DataNavigator"
                                             }; //"DevExpress.XtraWizard.WizardControl" };

            cntrl = Find_Control(tForm, "", TableIPCode, controls);
            if (cntrl != null)
            {
                return cntrl;
            }
            return null;
        }

        public int Find_Control_Type(Control cntrl)
        {
            int i = 0;

            if (cntrl.ToString().IndexOf("GridControl") > -1) // "DevExpress.XtraGrid.GridControl")
                i = v.obj_vw_GridView;

            if (cntrl.ToString().IndexOf("BandedGridView") > -1)  // "DevExpress.XtraGrid.Views.BandedGrid.BandedGridView")
                i = v.obj_vw_BandedGridView;

            if (cntrl.ToString().IndexOf("AdvBandedGridView") > -1) // "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                i = v.obj_vw_AdvBandedGridView;

            if (cntrl.ToString().IndexOf("LayoutView") > -1)
                i = v.obj_vw_GridLayoutView;

            if (cntrl.ToString().IndexOf("DataLayoutControl") > -1) // "DevExpress.XtraDataLayout.DataLayoutControl")
                i = v.obj_vw_DataLayoutView;

            if (cntrl.ToString().IndexOf("HtmlEditorsView") > -1) // web üzerinde
                i = v.obj_vw_HtmlEditorsView;

            if (cntrl.ToString().IndexOf("VGridControl") > -1) // "DevExpress.XtraVerticalGrid.VGridControl")
            {
                if (((DevExpress.XtraVerticalGrid.VGridControl)cntrl).LayoutStyle == LayoutViewStyle.SingleRecordView)
                    i = v.obj_vw_VGridSingle;

                if (((DevExpress.XtraVerticalGrid.VGridControl)cntrl).LayoutStyle == LayoutViewStyle.MultiRecordView)
                    i = v.obj_vw_VGridMulti;
            }

            if (cntrl.ToString().IndexOf("TreeList") > -1) // "DevExpress.XtraTreeList.TreeList")
                i = v.obj_vw_TreeListView;

            if (cntrl.ToString().IndexOf("DataNavigator") > -1) // "DevExpress.XtraEditors.DataNavigator")
                i = v.obj_DataNavigator;

            if (cntrl.ToString().IndexOf("WizardControl") > -1) // "DevExpress.XtraWizard.WizardControl")
                i = v.obj_vw_WizardControl;

            if (i == 0)
            {
                MessageBox.Show(cntrl.ToString() + v.ENTER + "Bu control tanımlı değil...", "Find_Control_Type");
            }
            return i;
        }

        public Control Find_PopupContainerEdit(PopupContainerEdit popupContainerEdit, string Name)
        {
            int i2 = popupContainerEdit.Properties.PopupControl.Controls.Count;
            Control c = null;
            for (int i = 0; i < i2; i++)
            {
                c = popupContainerEdit.Properties.PopupControl.Controls[i];

                //v.ControlList = v.ControlList + c.ToString() + " ( " + c.Name.ToString() + " )" + v.ENTER;

                if (c.Name.ToString() == Name)
                {
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// İstenen isimde klasör açar.Aynı isimde klasör var ise işlem yapmaz.
        /// </summary>
        /// <param name="KlasorIsim">Yazılacak olan klasör ismi C:\\ altında kontrol edilir.Klasör yok ise oluşturulur, var ise işlem yapılmaz.</param>
        public string Find_Path(string PathName)
        {
            if ((Directory.Exists(v.EXE_PATH + "\\" + PathName) == false))
            {
                Directory.CreateDirectory(v.EXE_PATH + "\\" + PathName);
            }
            return v.EXE_PATH + "\\" + PathName + "\\";
        }

        public string Find_SourceValueField(Form tForm, string tableIPCode)
        {
            // toperand_type = t.Set(Row["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);
            // 21, Source Value Field

            Control c = Find_Control_Tag(tForm, "21");
            string fieldAndValue = "";

            if (c != null)
            {
                if (c.AccessibleName == tableIPCode)
                {
                    //Column_MesajTipiId
                    string name = c.Name.Replace("Column_", "");
                    string value = "";
                    if (c.ToString() == "DevExpress.XtraEditors.ImageComboBoxEdit")
                        value = ((DevExpress.XtraEditors.ImageComboBoxEdit)c).EditValue.ToString();
                    fieldAndValue = " " + name + " = " + value + " ";
                }
            }

            return fieldAndValue;
        }


        #endregion Control

        #region DataSet

        // Nedenini almadım ! ama daha önce bu çalışıyordu
        // An unhandled exception of type 'System.StackOverflowException' occurred in Project Manager V3.exe
        //
        //public Boolean Find_DataSet(Form tForm, ref DataSet dsData, ref DataNavigator tDataNavigator, string TableIPCode)
        //{
        //    Boolean sonuc = false;
        //    sonuc = Find_DataSet(tForm, ref dsData, ref tDataNavigator, TableIPCode);
        //    return sonuc;
        //}

        //public Boolean Find_DataSet(Form tForm, ref DataSet dsData, ref DataNavigator tDataNavigator, string TableIPCode, string Kim)

        public Boolean Find_DataSet(Form tForm, ref DataSet dsData, ref DataNavigator tDataNavigator, string TableIPCode)
        {
            Boolean sonuc = false;

            // daha önce tespit edilmiş demektir
            if (tDataNavigator != null) return false;

            tDataNavigator = Find_DataNavigator(tForm, TableIPCode);//, Kim);

            if (tDataNavigator != null)
            {
                object tDataTable = tDataNavigator.DataSource;
                if (tDataTable != null)
                {
                    dsData = ((DataTable)tDataTable).DataSet;
                    sonuc = true;
                }
            }
            return sonuc;
        }

        public DataSet Find_DataSet(Form tForm, string CmpName, string TableIPCode, string Arayan)
        {
            // Dikkat : Direk olarak DataSet bulunamıyor
            // Ya CompanentName ile yada TableIPCode uyan bir Control tespit ediliyor
            // Bu Control ün önce DataTable na sonrada onun DataSet ine ulaşılıyor

            string function_name = "Find_DataSet";
            Takipci(function_name, "", '{');

            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraGrid.GridControl",
                                               "DevExpress.XtraVerticalGrid.VGridControl",
                                               "DevExpress.XtraDataLayout.DataLayoutControl",
                                               "DevExpress.XtraTreeList.TreeList",
                                               "DevExpress.XtraEditors.DataNavigator",
                                               "DevExpress.XtraBars.Navigation.NavigationPane" };
                                               //"DevExpress.XtraTab.XtraTabPage",
                                               //"DevExpress.XtraTab.XtraTabControl"
                                               //"DevExpress.XtraWizard.WizardControl" 

            cntrl = Find_Control(tForm, CmpName, TableIPCode, controls);

            //Takipci(function_name, "", '}');

            if (cntrl != null)
            {
                object tDataTable = new object();

                // Page içindeki view yapan controlu al
                // o controlde aşağıdakilerden birisidir
                if (cntrl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
                    cntrl = ((DevExpress.XtraBars.Navigation.NavigationPane)cntrl).Controls[0].Controls[0];

                if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    tDataTable = ((DevExpress.XtraGrid.GridControl)cntrl).DataSource;

                if (cntrl.GetType().ToString() == "DevExpress.XtraVerticalGrid.VGridControl")
                    tDataTable = ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).DataSource;

                if (cntrl.GetType().ToString() == "DevExpress.XtraDataLayout.DataLayoutControl")
                    tDataTable = ((DevExpress.XtraDataLayout.DataLayoutControl)cntrl).DataSource;

                if (cntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                    tDataTable = ((DevExpress.XtraTreeList.TreeList)cntrl).DataSource;

                if (cntrl.GetType().ToString() == "DevExpress.XtraEditors.DataNavigator")
                    tDataTable = ((DevExpress.XtraEditors.DataNavigator)cntrl).DataSource;

                

                if (cntrl.GetType().ToString() == "DevExpress.XtraWizard.WizardControl")
                {
                    // üzerindeki DevExpress.XtraDataLayout.DataLayoutControl ait datasource ulaşılıyor 
                    MessageBox.Show("DevExpress.XtraWizard.WizardControl  için datasource metodu ???");
                }

                ////DevExpress.XtraGrid.Views.Layout.LayoutView
                if (tDataTable != null)
                {
                    if (tDataTable.ToString() != "System.Object")
                    {
                        DataSet tdsData = ((DataTable)tDataTable).DataSet;

                        //************************************************************
                        // gerek yok zaten kendisi herşeyi atıyormuş
                        // eğer bilgiler uçuyorsa başka bir yerde Namespace = "" vardır 
                        //************************************************************

                        // üzerine yamadığımız bilgilerde transfer olsun

                        //tdsData.DataSetName = ((DataTable)tDataTable).DataSet.DataSetName;
                        //tdsData.Namespace = ((DataTable)tDataTable).DataSet.Namespace;

                        //if (tdsData.Tables.Count > 0)
                        //{
                        //    tdsData.Tables[0].Namespace = ((DataTable)tDataTable).DataSet.Tables[0].Namespace;
                        //}

                        return tdsData;
                    }
                    else return null;
                }
                else return null;
            }
            else
            {
                if ((CmpName.IndexOf("tMyDataNavigator_") == -1) && (Arayan != ""))
                {
                    if (TableIPCode != "null")
                        MessageBox.Show("DİKKAT : [ " + TableIPCode + " ] için DataControl tespit edilemedi ..." + v.ENTER +
                                        "Kim Arıyor : " + Arayan, function_name);
                    if (TableIPCode == "")
                        MessageBox.Show("DİKKAT : DataControl tespit yapılacak fakat TableIPCode parametresi boş geldi ..." + v.ENTER +
                                        "Kim Arıyor : " + Arayan, function_name);
                }

                return null;
            }

        }

        public void Find_DataSet_List(Form tForm, DataSet dsIPList)
        {
            tToolBox t = new tToolBox();

            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            List<string> list = new List<string>();
            t.Find_Control_List(tForm, list, controls);

            string myProp = string.Empty;
            string TableIPCode = string.Empty;
            string TableCaption = string.Empty;
            string softCode = "";
            string projectCode = "";
            string TableCode = string.Empty;
            string IPCode = string.Empty;

            TableRowDelete(dsIPList, 0);

            int i = 1;
            foreach (string value in list)
            {
                //s = s + value + v.ENTER;
                cntrl = t.Find_Control(tForm, value, "", controls);

                if (cntrl != null)
                {
                    if (((DevExpress.XtraEditors.DataNavigator)cntrl).AccessibleName != null)
                    {
                        object tDataTable = ((DevExpress.XtraEditors.DataNavigator)cntrl).DataSource;
                        DataSet dsData = ((DataTable)tDataTable).DataSet;

                        if (dsData != null)
                        {
                            myProp = dsData.Namespace;
                            TableIPCode = t.MyProperties_Get(myProp, "TableIPCode:");
                            TableCaption = t.MyProperties_Get(myProp, "TableCaption:");

                            TableIPCode_Get(TableIPCode, ref softCode, ref projectCode, ref TableCode, ref IPCode);

                            DataRow row = dsIPList.Tables[0].NewRow();
                            row["ID"] = i;
                            row["TABLECODE"] = TableCode;
                            row["IPCODE"] = IPCode;
                            row["TABLEIPCODE"] = TableIPCode;
                            row["TABLECAPTION"] = TableCaption;

                            dsIPList.Tables[0].Rows.Add(row);
                            i++;
                        }
                    }
                }
            }
        }

        public string Find_TableState(DataSet ds, DataNavigator dN, string TableName, Int16 TableNo)
        {
            string id_value = string.Empty;
            string State = string.Empty;
            try
            {
                // sıfırın column her zaman ID veya REF_ID kabul edildiği için
                if (IsNotNull(TableName))
                    id_value = ds.Tables[TableName].Rows[dN.Position][0].ToString();
                if ((IsNotNull(TableName) == false) &&
                    (TableNo > -1))
                    id_value = ds.Tables[TableNo].Rows[dN.Position][0].ToString();
            }
            catch
            {
                return ""; // dataset üzerinde veri yoksa burada hata olarak yakalanır
            }

            if ((id_value == "") || (id_value == "-999"))
                State = "dsInsert";
            else State = "dsEdit";

            return State;
        }

        #endregion DataSet

        #region DataNavigator
        public DataNavigator Find_DataNavigator(Form tForm, string TableIPCode)
        {
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            cntrl = Find_Control(tForm, "", TableIPCode, controls);
            
            if (cntrl != null)
            {
                DataNavigator tDataNavigator = ((DevExpress.XtraEditors.DataNavigator)cntrl);
                return tDataNavigator;
            }
            else
            {
                return null;
            }
        }
        #endregion DataNavigator

        #region DataNavigator_Position

        public int Find_DataNavigator_Position(Form tForm, string TableIPCode)
        {
            string function_name = "tDataNavigator_Position_Find";
            Takipci(function_name, "", '{');

            DataNavigator tDataNavigator = null;

            tDataNavigator = Find_DataNavigator(tForm, TableIPCode);//, function_name);

            Takipci(function_name, "", '}');

            if (tDataNavigator != null)
            {
                int i = -1;

                try
                {
                    object x = tDataNavigator.Tag;
                    i = ((int)x);
                }
                catch
                {
                    i = -1;
                }

                if (i == -1) i = tDataNavigator.Position;

                if (i != tDataNavigator.Position)
                {
                    i = tDataNavigator.Position;
                    tDataNavigator.Tag = i;
                }

                return i;
            }
            else
            {
                return -1;
            }

        }

        public int Find_DataNavigator_Position(Form tForm, DataSet dsData, string TableIPCode)
        {
            //string function_name = "Find_DataNavigator_Position-2";
            int snc = -1;
            if (dsData != null)
            {
                if (dsData.Tables[0].Rows.Count == 1)
                {
                    snc = 0;
                }
                else
                {
                    DataNavigator tDataNavigator = Find_DataNavigator(tForm, TableIPCode);//, function_name);
                    if (tDataNavigator != null)
                    {
                        snc = tDataNavigator.Position;
                    }
                }
            }

            return snc;
        }

        #endregion DataNavigator_Position

        #region Find_dBLongName
        public string Find_dBLongName(string DatabaseName)
        {
            string s = string.Empty;
            /*
        public enum dBaseNo : byte
        {
            None = 0,
            Master = 1, 
            Manager = 2,
            UstadCrm = 3, 
            Project = 4,
            WebCrm = 5,
            NewDatabase = 6,
            publishManager = 7 
        }
            */

            // "master"
            if ((DatabaseName.ToUpper() == "MASTER") ||
                (DatabaseName == v.dBaseNo.Master.ToString()) ||
                (DatabaseName == "1"))
                s = v.db_MASTER_DBNAME;

            //  "MANAGERSERVER"
            if ((v.active_DB.managerDBName.ToUpper() == DatabaseName.ToUpper()) ||
                (DatabaseName == v.dBaseNo.Manager.ToString()) ||
                (DatabaseName == "2"))
                s = v.active_DB.managerDBName;

            //  "UstadCRM"
            if ((v.active_DB.ustadCrmDBName.ToUpper() == DatabaseName.ToUpper())  ||
                (DatabaseName == "3"))
                s = v.active_DB.ustadCrmDBName;

            //  proje adı
            if ((v.active_DB.projectDBName.ToUpper() == DatabaseName.ToUpper()) ||
                (DatabaseName == v.dBaseNo.Project.ToString()) ||
                (DatabaseName == "4") ||
                (DatabaseName == ""))
                s = v.active_DB.projectDBName;

            //  "Local"
            if ((v.active_DB.localDBName.ToUpper() == DatabaseName.ToUpper()) ||
                (DatabaseName == "5"))
                s = v.active_DB.localDBName;

            //  "publishManager - web"
            if ((v.publishManager_DB.databaseName.ToUpper() == DatabaseName.ToUpper()) ||
                (DatabaseName == "7"))
                s = v.publishManager_DB.databaseName;

            //  "aktarılacak database"
            if ((v.active_DB.projectDBName.ToUpper() == DatabaseName.ToUpper()) ||
                (DatabaseName == "8"))
                s = v.source_DB.databaseName;

            if (IsNotNull(s))
            {
                return s;
            }
            else
            {
                if (DatabaseName != "null")
                    MessageBox.Show("DİKKAT : [ " + DatabaseName + " ] için Database Name tespit edilemedi ...");

                return "";
            }
        }
        #endregion Find_dBLongName

        #region Find_Record
        
        public Boolean Find_Record(v.dBaseNo dBaseNo, ref DataSet ds, string SQL)
        {
            // SqlConnection SqlConn buna gerek kalmadı silebilirsiniz

            Boolean sonuc = false;

            if (IsNotNull(SQL))
            {
                SQL_Read_Execute(dBaseNo, ds, ref SQL, "TABLE1", "");

                sonuc = IsNotNull(ds);
            }
            
            // false dönerse aranın kayıt yok
            // true  dönerse aranan kayıt mevcut
            return sonuc;
        }
        
        public bool Find_EditState(Form tForm, string TableIPCode)
        {
            bool onay = false;

            DataSet dsSource = null;
            DataNavigator dNSource = null;
            Find_DataSet(tForm, ref dsSource, ref dNSource, TableIPCode);

            string id_value = "";
            if (IsNotNull(dsSource))
            {
                id_value = dsSource.Tables[0].Rows[dNSource.Position][0].ToString();
                if (id_value != "") onay = true;
            }

            return onay;
        }
        
        #endregion Find_Record

        #region Diğerleri >>

        public void preparing_FirmILAdi_IlceAdi()
        {
            if (IsNotNull(v.ds_ILList))
            {
                int length = v.ds_ILList.Tables[0].Rows.Count;
                for (int i = 0; i < length; i++)
                {
                    if (v.ds_ILList.Tables[0].Rows[i]["IlKodu"].ToString() == v.tMainFirm.IlKodu)
                    {
                        v.tMainFirm.IlAdi = v.ds_ILList.Tables[0].Rows[i]["IlAdiBUYUK"].ToString();
                        break;
                    }
                }
            }
            if (IsNotNull(v.ds_IlceList))
            {
                int length = v.ds_IlceList.Tables[0].Rows.Count;
                /// 0 da ilçe seçiniz var onun için 1 den başladı
                for (int i = 1; i < length; i++)
                {
                    if (v.ds_IlceList.Tables[0].Rows[i]["IlceKodu"].ToString() == v.tMainFirm.IlceKodu)
                    {
                        v.tMainFirm.IlceAdi = v.ds_IlceList.Tables[0].Rows[i]["IlceAdi"].ToString();
                        break;
                    }
                }
            }


        }
        public Control Find_NumberControlDisplay(Form tForm, string cmpName)
        {
            Control cntrl2 = null;
            Control cntrl = Find_Control(tForm, cmpName);
            if (cntrl != null)
            {
                if (cntrl.GetType().ToString() == "DevExpress.XtraEditors.PanelControl")
                {
                    cntrl2 = ((DevExpress.XtraEditors.PanelControl)cntrl).Controls[0];
                    if (cntrl2.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
                    {
                        return cntrl2;
                    }
                }
            }
            return cntrl2;
        }

        // görevli anahtar
        public bool findAttendantKey(System.Windows.Forms.KeyEventArgs e)
        {
            bool onay = false;
            
            if (((e.KeyCode >= Keys.F1) &&
                 (e.KeyCode <= Keys.F24)) |
                 (e.KeyCode == Keys.Enter) |
                ((e.KeyCode == Keys.Down) && (e.Modifiers == Keys.Shift)) |
                ((e.KeyCode == Keys.Delete) && (e.Modifiers == Keys.Control))
               ) onay = true;

            return onay;
        }

        //direction key
        public bool findDirectionKey(KeyEventArgs e)// yön anahtarları
        {
            bool onay = false;

            if ((e.KeyCode == Keys.Left) |
                (e.KeyCode == Keys.Right) |
                (e.KeyCode == Keys.Up) |
                (e.KeyCode == Keys.Down) |
                (e.KeyCode == Keys.PageUp) |
                (e.KeyCode == Keys.PageDown) |
                (e.KeyCode == Keys.Home) |
                (e.KeyCode == Keys.End) |
                (e.KeyCode == Keys.Escape)) onay = true;

            return onay;
        }

        public bool findReturnKey(KeyEventArgs e)// görevli anahtar
        {
            bool onay = false;

            if ((e.KeyCode == Keys.Enter) |
                (e.KeyCode == Keys.Return)) onay = true;

            return onay;
        }

        public Image Find_Glyph(string glyphName)
        {
            if ((IsNotNull(v.ds_Icons) == false) || (IsNotNull(glyphName) == false))
                return null;

            //ImageName

            if (glyphName == "KAYDET16") glyphName = "30_341_Save_16x16";

            if (glyphName == "SAVE16") glyphName = "40_424_Save_16x16";
            if (glyphName == "SAVE32") glyphName = "40_424_Save_32x32";
            if (glyphName == "CLOSE16") glyphName = "40_401_Close_16x16";
            if (glyphName == "CLOSE32") glyphName = "40_401_Close_32x32";

            if (glyphName == "YENIHESAP16") glyphName = "40_401_AddFile_16x16";
            if (glyphName == "YENIVAZGEC16") glyphName = "40_401_DeleteList2_16x16";


            if (glyphName == "DELETE16") glyphName = "40_408_Delete_16x16";
            if (glyphName == "DELETE32") glyphName = "40_408_Delete_32x32";

            if (glyphName == "SIHIRBAZGERI16") glyphName = "40_418_Backward_16x16";
            if (glyphName == "SIHIRBAZDEVAM16") glyphName = "40_418_Forward_16x16";

            if (glyphName == "NAVGERI16") glyphName = "40_418_Backward_16x16";
            if (glyphName == "NAVILERI16") glyphName = "40_418_Forward_16x16";


            if (glyphName == "") glyphName = "";
            if (glyphName == "") glyphName = "";

            if (glyphName == "") glyphName = "";
            if (glyphName == "") glyphName = "";

            int i2 = v.ds_Icons.Tables[0].Rows.Count;
            int i3 = -1;
            for (int i = 0; i < i2; i++)
            {
                if (v.ds_Icons.Tables[0].Rows[i]["GLYPH_NAME"].ToString() == glyphName)
                {
                    i3 = i;
                    break;
                }
            }

            if (i3 > -1)
            {
                byte[] img32 = (byte[])v.ds_Icons.Tables[0].Rows[i3]["GLYPH"];
                if ((img32 != null) && (i3 > -1))
                {
                    MemoryStream ms = new MemoryStream(img32);
                    return Image.FromStream(ms);
                }
            }
            return null;
        }

        public string Find_TableIPCode(Form tForm, string findTableName)
        {
            string TableIPCode = "";
            string tName = "";
                        
            List<string> list = new List<string>();
            Find_DataNavigator_List(tForm, ref list);

            #region DataNavigator Listesi
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };

            foreach (string value in list)
            {
                cntrl = Find_Control(tForm, value, "", controls);
                if (cntrl != null)
                {
                    DataNavigator dN = (DevExpress.XtraEditors.DataNavigator)cntrl;

                    if (dN.DataSource != null)
                    {
                        tName = dN.AccessibleDefaultActionDescription;
                        if (tName == findTableName)
                        {
                            TableIPCode = dN.AccessibleName.ToString();
                            break;
                        }
                    }

                } // if cntrl != null
            }//foreach

            #endregion DataNavigator Listesi

            return TableIPCode;
        }

        public string Find_FirstTableIPCode(Form tForm)
        {
            string TableIPCode = "";

            List<string> list = new List<string>();
            Find_DataNavigator_List(tForm, ref list);

            #region DataNavigator Listesi
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };

            foreach (string value in list)
            {
                cntrl = Find_Control(tForm, value, "", controls);
                if (cntrl != null)
                {
                    DataNavigator dN = (DevExpress.XtraEditors.DataNavigator)cntrl;

                    if (dN.DataSource != null)
                    {
                        TableIPCode = dN.AccessibleName.ToString();

                        if (v.formLastActiveControl == null)
                            tFormActiveView(tForm, TableIPCode);

                        break;
                    }

                } // if cntrl != null
            }//foreach

            #endregion DataNavigator Listesi

            return TableIPCode;
        }

        public int Find_DataNavigator_Count(Form tForm)
        {
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            List<string> list = new List<string>();
            Find_Control_List(tForm, list, controls, "");
            return list.Count;
        }

        public void Find_DataNavigator_List(Form tForm, ref List<string> list)
        {
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            //List<string> list = new List<string>();
            Find_Control_List(tForm, list, controls, "");
        }

        public void Find_DataNavigator_List(Form tForm, ref List<string> list, string tabPageName)
        {
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            //List<string> list = new List<string>();
            Find_Control_List(tForm, list, controls, tabPageName);
        }

        public void Find_DataNavigator_ButtonLink(Form tForm, string TableIPCode, string Button_Click_Type)
        {
            tToolBox t = new tToolBox();
            string function_name = "Find_DataNavigator_ButtonLink";
            t.Takipci(function_name, "", '{');

            string ButtonName = string.Empty;

            ButtonName = Find_ButtonName(tForm, TableIPCode, Button_Click_Type);

            if (ButtonName != string.Empty)
            {
                //tButton bt = new tButton();
                //bt.tButtonClick(tForm, ButtonName);
            }
            else
            {

                if ((Button_Click_Type != "11") && // Çıkış değilse
                    (Button_Click_Type != "999"))
                {
                    MessageBox.Show(
                        "DİKKAT : Butona ait bağlantı Link'i bulunamadı ..." + v.ENTER2 +
                        "1. Buton tanımlanmamış olabilir." + v.ENTER +
                        "2. Buton üzerindeki tanımlar eksik olabilir." + v.ENTER +
                        "3. ExternelTableIPCode olabilir. "
                        );
                }

                if ((Button_Click_Type == "11") && // Çıkış
                    (Button_Click_Type != "999"))
                {
                    tForm.Close();
                }
            }

            t.Takipci(function_name, "", '}');
        }

        public string Find_ButtonName(Form tForm, string TableIPCode, string Button_Click_Type)
        {
            string function_name = "Find_ButtonName";
            Takipci(function_name, "", '{');

            string snc = string.Empty;
            string btn = string.Empty;
            string ButtonName = string.Empty;

            // 1. formun üzerinde create sırasında atanmış olan button hakkındaki bilgiler okunur
            snc = Find_myFormBox_Value(tForm, "");

            int i1 = 0;
            int i2 = 0;

            string s1 = "TableIPCode1:" + TableIPCode;
            string s2 = "=ButtonClickType:" + Button_Click_Type;

            while (snc.IndexOf("//<btn_1>") > -1)
            {
                btn = BeforeGet_And_AfterClear(ref snc, "//<btn_1>", true);

                i1 = btn.IndexOf(s1);
                i2 = btn.IndexOf(s2);

                if ((i1 > -1) && (i2 > -1))
                {
                    BeforeGet_And_AfterClear(ref btn, "//<btn_0>", false);
                    ButtonName = BeforeGet_And_AfterClear(ref btn, "=Caption:", false);
                    break;
                }
            }

            Takipci(function_name, "", '}');

            return ButtonName;

            /*
            //<btn_0>
            tBarButtonItem_84=Caption:Fiş Aç;
            tBarButtonItem_84=FormCode:22;
            tBarButtonItem_84=SourceTableIPCode1:TABLE_1.t11;
            tBarButtonItem_84=SourceFieldName11:HAREKET_ID;
            tBarButtonItem_84=SourceFieldValue11:null;
            tBarButtonItem_84=TargetTableIPCode1:TABLE_1.t12;
            tBarButtonItem_84=TargetFieldName11:HAREKET_ID;
            tBarButtonItem_84=TargetFieldValue11:null;
            tBarButtonItem_84=ButtonClickType:17;
            tBarButtonItem_84=ProcedureApproval:null;
            tBarButtonItem_84=LookupCode:null;
            tBarButtonItem_84=DefaultValue:null;
            tBarButtonItem_84=END:
            //<btn_1>
            */
        }

        public Control Find_SimpleButton(Form tForm, string buttonName, string tableIPCode)
        {
            /*
            "dragDownButton_ek1"
            "checkButton_ek1"
            "simpleButton_ek7"
            "simpleButton_ek6"
            "simpleButton_ek5"
            "simpleButton_ek4"
            "simpleButton_ek3"
            "simpleButton_ek2"
            "simpleButton_ek1"

            "simpleButton_Coll"
            "simpleButton_Exp"
            "simpleButton_onay_Kaldir"
            "simpleButton_onay_Ekle"
            "simpleButton_yazici"

            "simpleButton_en_basa"
            "simpleButton_onceki_syf"
            "simpleButton_onceki"
            "simpleButton_sonraki"
            "simpleButton_sonraki_syf"
            "simpleButton_en_sona"

            "simpleButton_sil_liste"
            "simpleButton_sil_belge"
            "simpleButton_sil_hesap"
            "simpleButton_sil_kart"
            "simpleButton_sil_satir"

            "simpleButton_kaydet_yeni"
            "simpleButton_kaydet"
            "simpleButton_kaydet_devam"
            "simpleButton_kaydet_cik"

            "simpleButton_yeni_kart"
            "simpleButton_yeni_hesap"
            "simpleButton_yeni_belge"
            "simpleButton_yeni_alt_hesap"
            "simpleButton_yeni_kart_satir"
            "simpleButton_yeni_hesap_satir"
            "simpleButton_yeni_belge_satir"
            "simpleButton_yeni_alt_hesap_satir"

            "simpleButton_goster"
            "simpleButton_kart_ac"
            "simpleButton_hesap_ac"
            "simpleButton_belge_ac"
            "simpleButton_resim_edit"
            "simpleButton_report_design"

            "simpleButton_sec"
            "simpleButton_listeye_ekle"
            "simpleButton_liste_hazirla"
            "simpleButton_listele"
            "simpleButton_sihirbaz_geri"
            "simpleButton_sihirbaz_devam"
            "simpleButton_cikis"

            */
            Control cntrl = null;
            string[] controls = new string[] { };
            cntrl = Find_Control(tForm, buttonName, tableIPCode, controls);
            return cntrl;
        }

        public void myFormBox_Set(Form tForm, string AddValue)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, "tMyFormBox", "", controls);

            if (c != null)
            {
                ((DevExpress.XtraEditors.MemoEdit)c).EditValue =
                    ((DevExpress.XtraEditors.MemoEdit)c).EditValue + v.ENTER + AddValue;
            }
        }

        public string Find_myFormBox_Value(Form tForm, string ButtonName)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, "tMyFormBox", "", controls);
            string s = string.Empty;

            // ButtonName boş ise tMyFormBox üzerindeki tüm bilgiler gönderilecek
            //            boş değilse sadece ButtonName ait bilgiler gönderilecek

            if (c != null)
            {
                if (((DevExpress.XtraEditors.MemoEdit)c).EditValue != null)
                {
                    s = ((DevExpress.XtraEditors.MemoEdit)c).EditValue.ToString();

                    int i1 = 0;
                    int i2 = 0;
                    if ((ButtonName != string.Empty) || (ButtonName != ""))
                    {
                        i1 = s.IndexOf(ButtonName);
                        i2 = s.LastIndexOf(ButtonName + "=END:");

                        if ((i1 > -1) && (i2 > -1))
                            s = s.Substring(i1, i2 - i1);
                    }
                }
            }

            return s;
        }

        public void Find_TableIPCode(string snc,
                                     ref string myTargetTableIPCode, ref string mySourceTableIPCode)
        {
            // bir form üzerinde birden fazla TableIP olduğu zaman 
            // örnek birden fazla detail tablo olduğu zaman hangisi seçecek ????

            string SourceTableIPCode1 = string.Empty;
            string TargetTableIPCode1 = string.Empty;
            string DetailTableIPCode1 = string.Empty;

            string TargetFieldName11 = string.Empty;
            string TargetFieldName12 = string.Empty;
            string TargetValue1 = string.Empty;
            string TargetValue2 = string.Empty;

            if (snc.Length > 0)
            {
                SourceTableIPCode1 = MyProperties_Get(snc, "=SourceTableIPCode1:");
                TargetTableIPCode1 = MyProperties_Get(snc, "=TargetTableIPCode1:");
                DetailTableIPCode1 = MyProperties_Get(snc, "=DetailTableIPCode1:");
            }

            if ((SourceTableIPCode1 == "null") && (SourceTableIPCode1 == "")) SourceTableIPCode1 = string.Empty;
            if ((TargetTableIPCode1 == "null") && (TargetTableIPCode1 == "")) TargetTableIPCode1 = string.Empty;
            if ((DetailTableIPCode1 == "null") && (DetailTableIPCode1 == "")) DetailTableIPCode1 = string.Empty;


            // my ile başlayan değerler 
            if ((TargetTableIPCode1 == string.Empty) &&
                (snc.LastIndexOf("//<myStart>") > -1))
            {
                TargetTableIPCode1 = MyProperties_Get(snc, "myTargetTableIPCode1=");
                TargetFieldName11 = MyProperties_Get(snc, "myTargetFieldName11=");
                TargetFieldName12 = MyProperties_Get(snc, "myTargetFieldName12=");
                TargetValue1 = MyProperties_Get(snc, "myTargetValue1=");
                TargetValue2 = MyProperties_Get(snc, "myTargetValue2=");
            }

            mySourceTableIPCode = string.Empty;

            if ((SourceTableIPCode1 != "") && (TargetTableIPCode1 == "") && (DetailTableIPCode1 == ""))
                myTargetTableIPCode = SourceTableIPCode1;

            if ((SourceTableIPCode1 == "") && (TargetTableIPCode1 != "") && (DetailTableIPCode1 == ""))
                myTargetTableIPCode = TargetTableIPCode1;

            if ((SourceTableIPCode1 == "") && (TargetTableIPCode1 == "") && (DetailTableIPCode1 != ""))
                myTargetTableIPCode = DetailTableIPCode1;

            if ((SourceTableIPCode1 != "") && (TargetTableIPCode1 != ""))
            {
                myTargetTableIPCode = TargetTableIPCode1;
                mySourceTableIPCode = SourceTableIPCode1;
            }

            if ((TargetTableIPCode1 != "") && (DetailTableIPCode1 != ""))
            {
                myTargetTableIPCode = DetailTableIPCode1;
                mySourceTableIPCode = TargetTableIPCode1;
            }
        }

        //public string Find_Table_Ref_FieldName(DataSet dsIPFields)
        public string Find_Table_Ref_FieldName(DataTable dsIPFields)
        {
            string fname = string.Empty;
            
            //foreach (DataRow row in dsIPFields.Tables[0].Rows)
            foreach (DataRow row in dsIPFields.Rows)
            {
                //if (row["LKP_FIELD_NO"].ToString() == "1")
                if ((row["LKP_FAUTOINC"].ToString() == "True") ||
                    (row["LKP_FAUTOINC"].ToString() == "1"))
                {
                    fname = row["LKP_FIELD_NAME"].ToString();
                    break;
                }
            }

            return fname;
        }
        
        public bool Find_Value_In_Field(Form tForm, string tableIPCode, string fieldName, string findValue)
        {
            bool onay = false;

            /// Buraya geldiysen tablonun üzerinde LKP_ONAY field var 
            /// ve kullanıcı seçim yapmış mı yapmamış mı onu kontrol ediyoruz
            /// 
            DataSet ds = null;
            DataNavigator dN = null;
            Find_DataSet(tForm, ref ds, ref dN, tableIPCode);

            if (IsNotNull(ds) != false)
            {
                string value = "";
                int length = ds.Tables[0].Rows.Count;
                for (int i = 0; i < length; i++)
                {
                    value = ds.Tables[0].Rows[i][fieldName].ToString();
                    if (value.IndexOf(findValue) > -1)
                    {
                        onay = true;
                        break;
                    }
                }
            }
            return onay;
        }
        public bool Find_FieldName(Form tForm, string tableIPCode, string findFieldName)
        {
            bool onay = false;

            DataSet ds = null;
            DataNavigator dN = null;
            Find_DataSet(tForm, ref ds, ref dN, tableIPCode);

            if (IsNotNull(ds) != false)
            {
                string columnName = "";
                int length = ds.Tables[0].Columns.Count;
                for (int i = 0; i < length; i++)
                {
                    columnName = ds.Tables[0].Columns[i].ColumnName;
                    if (columnName.IndexOf(findFieldName) > -1)
                    {
                        onay = true;
                        break;
                    }
                }
            }
            return onay;
        }
        public bool Find_TableFields(Form tForm, DataSet dsData)
        {
            string myProp = dsData.Namespace;
            string tableName = MyProperties_Get(myProp, "TableName:");// + "_FIELDS";

            DataTable dt = v.ds_MsTableFields.Tables[tableName];
            if ((dt == null) && (tForm != null))
            {
                // table fields list yok ise şansını dene
                //
                vTable vt = new vTable();
                Preparing_DataSet(tForm, dsData, vt);
                               
                preparing_MsTableFields(vt);
                
                // yine kontrol fields gelmiş mi
                dt = v.ds_MsTableFields.Tables[tableName];

                // gelen datasete de bakma 
                // aranan tablo fieldleri muhakkak v.ds_MsTableFields içinde olmalı
                //if (dt == null)
                //    dt = dsData.Tables[vt.TableIPCode];

                // halen yok ise elveda
                if (dt == null)
                {
                    v.Kullaniciya_Mesaj_Var = "Dikkat field listesi yok : " + tableName + " " + vt.TableIPCode + " için ";
                    v.timer_Kullaniciya_Mesaj_Var_.Enabled = true;
                    return false;
                }
            }

            if (dt.Rows.Count == 0) return false;
            dt.Dispose();
            return true;
        }
        public int Find_Field_Type_Id(DataSet dsData, string fieldName, ref string displayFormat)
        {
            int ftype = 0;

            // fields tablsou varsa
            if (dsData != null)
            {
                if (dsData.Namespace != null)
                {
                    
                    int i2 = 0;
                    string function_name = "";
                    string myProp = dsData.Namespace.ToString();
                    string tableName = Set(MyProperties_Get(myProp, "=TableName:"), "", "TABLE1");
                    string TableIPCode = MyProperties_Get(myProp, "TableIPCode:");
                    //string TableFields = Table_Name + "_FIELDS";

                    if (Find_TableFields(null, dsData))
                    {
                        try
                        {
                            i2 = v.ds_MsTableFields.Tables[tableName].Rows.Count;
                            if (i2 == 0)
                            {
                                MessageBox.Show("DİKKAT : " + tableName + " isimli tablonun field listesi bulunamadı ... " + v.ENTER2 +
                                                "( TableFields[xxxx_FIELDS].Rows.Count = 0 )", function_name);
                                return ftype;
                            }
                        }
                        catch
                        {
                            MessageBox.Show("DİKKAT : " + tableName + " isimli tablonun field listesi bulunamadı ... " + v.ENTER2 +
                                            "( TableFields[xxxx_FIELDS].Rows.Count = 0 )", function_name);
                            i2 = 0;
                            return ftype;
                        }

                        string ValidationInsert = "";
                        string fForeing = "";
                        string fTrigger = "";
                        string fVisible = "";
                        string fname = "";
                        //string displayFormat = "";

                        for (int i = 0; i < i2; i++)
                        {
                            //fname = dsData.Tables[TableFields].Rows[i]["name"].ToString();
                            fname = v.ds_MsTableFields.Tables[tableName].Rows[i]["name"].ToString();
                            if (fname == fieldName)
                            {
                                //ftype = myInt32(dsData.Tables[TableFields].Rows[i]["user_type_id"].ToString());
                                ftype = myInt32(v.ds_MsTableFields.Tables[tableName].Rows[i]["user_type_id"].ToString());

                                if (v.active_DB.mainManagerDbUses) // tableFields + "2"
                                    OtherValues_Get(dsData, TableIPCode, fname,
                                    ref ValidationInsert, ref fForeing, ref fTrigger, ref displayFormat, ref fVisible);
                                else OtherValues_Get(v.ds_TableIPCodeFields, TableIPCode, fname,
                                     ref ValidationInsert, ref fForeing, ref fTrigger, ref displayFormat, ref fVisible);
                                break;
                            }
                        }
                    }
                }
            }
            return ftype;
        }

        public void OtherValues_Get(DataSet ds, saveVariables f)
        {
            f.fVisible = "False";
            f.ValidationInsert = "False";

            if (ds == null) return;
            if (ds.Tables == null) return;
            if (ds.Tables.Count < 1) return;

            string tableName_ = "";
            if (v.active_DB.mainManagerDbUses) // tableFields + "2"
                tableName_ = ds.DataSetName; // TapleIPCode
            else tableName_ = f.TableIPCode; //  f.tableName;   // normal tableName

            if (tableName_ == "NewDataSet" && ds.Namespace != null)
            {
                string myProp = ds.Namespace.ToString();
                tableName_ = MyProperties_Get(myProp, "TableName:");
            }

            int j = ds.Tables[tableName_].Rows.Count;
                       

            for (int i = 0; i < j; i++)
            {
                if (ds.Tables[tableName_].Rows[i]["LKP_FIELD_NAME"].ToString() == f.fname)
                {
                    f.ValidationInsert = Set(ds.Tables[tableName_].Rows[i]["VALIDATION_INSERT"].ToString(), "", "False");
                    f.fForeing = Set(ds.Tables[tableName_].Rows[i]["LKP_FFOREING"].ToString(), "", "False");
                    f.fTrigger = Set(ds.Tables[tableName_].Rows[i]["LKP_FTRIGGER"].ToString(), "", "False");
                    f.displayFormat = Set(ds.Tables[tableName_].Rows[i]["CMP_DISPLAY_FORMAT"].ToString(), "", "");
                    f.fVisible = Set(ds.Tables[tableName_].Rows[i]["CMP_VISIBLE"].ToString(), "", "");
                    f.fEnabled = Set(ds.Tables[tableName_].Rows[i]["CMP_ENABLED"].ToString(), "", "");
                    break;
                }
            }
        }

        public void OtherValues_Get(DataSet ds, string tableName, string fName,
            ref string ValidationInsert,
            ref string fForeing,
            ref string fTrigger,
            ref string displayFormat,
            ref string fVisible)
        {
            int j = ds.Tables[tableName].Rows.Count;

            for (int i = 0; i < j; i++)
            {
                //if (ds.Tables[tableName].Rows[i]["FIELD_NAME"].ToString() == fName)
                if (ds.Tables[tableName].Rows[i]["LKP_FIELD_NAME"].ToString() == fName)
                {
                    ValidationInsert = Set(ds.Tables[tableName].Rows[i]["VALIDATION_INSERT"].ToString(), "", "False");
                    //fForeing = Set(ds.Tables[tableName].Rows[i]["FFOREING"].ToString(), "", "False");
                    //fTrigger = Set(ds.Tables[tableName].Rows[i]["FTRIGGER"].ToString(), "", "False");
                    fForeing = Set(ds.Tables[tableName].Rows[i]["LKP_FFOREING"].ToString(), "", "False");
                    fTrigger = Set(ds.Tables[tableName].Rows[i]["LKP_FTRIGGER"].ToString(), "", "False");
                    displayFormat = Set(ds.Tables[tableName].Rows[i]["CMP_DISPLAY_FORMAT"].ToString(), "", "");
                    fVisible = Set(ds.Tables[tableName].Rows[i]["CMP_VISIBLE"].ToString(), "", "");
                    break;
                }
            }
            /*
            dbo.MS_FIELDS as d
               d.FIELD_NO
             , d.FIELD_NAME
             , d.FFOREING 
             , d.FTRIGGER 
             , d.PROP_EXPRESSION 
            
            dbo.MS_FIELDS_IP as e 
             , e.VALIDATION_INSERT
             , e.XML_FIELD_NAME
             , e.CMP_DISPLAY_FORMAT
             , e.CMP_EDIT_FORMAT
             , e.CMP_VISIBLE
             , e.DEFAULT_TYPE
            */
        }

        public VGridControl Find_VGridControl(Form tForm, string VGridName, string TableIPCode)
        {
            Control cntrl = new Control();
            string[] controls = new string[] { };

            if (IsNotNull(VGridName))
                cntrl = Find_Control(tForm, VGridName, "", controls);
            if (IsNotNull(TableIPCode))
                cntrl = Find_Control(tForm, "", TableIPCode, controls);

            if (cntrl != null)
            {
                VGridControl tVGrid = ((DevExpress.XtraVerticalGrid.VGridControl)cntrl);
                return tVGrid;
            }
            else
            {
                if (IsNotNull(VGridName))
                    MessageBox.Show("DİKKAT : [ " + VGridName + " ] VGrid tespit edilemedi ...", "Find_VGridControl");
                return null;
            }
        }

        public int Find_GotoRecord(DataSet dsData, string FieldName, string Value)
        {
            if (dsData == null)
                return -1;

            if ((IsNotNull(FieldName) == false) ||
                (IsNotNull(Value) == false) ||
                (dsData.Tables.Count < 1))
            {
                return -1;
            }

            // position tespit edilecek 
            int s = -1;
            int rc = dsData.Tables[0].Rows.Count;
            for (int i = 0; i < rc; i++)
            {
                if (dsData.Tables[0].Rows[i][FieldName].ToString() == Value)
                {
                    s = i;
                    break;
                }
            }

            // position döndürülüyor 
            return s;
        }

        public string Find_Kriter_Value(Form tForm, string TableIPCode, string FieldName, byte default_type)
        {
            tToolBox t = new tToolBox();

            string tValue = string.Empty;
            // default_type
            // 51,  'Kriter READ (Even.Bas)' 
            // 52,  'Kriter READ (Even.Bit)' 
            // 53,  'Kriter READ (Odd)'

            // VgridControl tespit ediliyor
            DevExpress.XtraVerticalGrid.VGridControl VGrid = t.Find_VGridControl(tForm, "", "KRITER_" + TableIPCode);


            if (VGrid != null)
            {
                string fullname = string.Empty;
                string softCode = "";
                string projectCode = "";
                string ttable_alias = string.Empty;
                string fname = string.Empty;

                string fvalue = string.Empty;
                string fvalue2 = string.Empty;
                //string tmpkriter = string.Empty;
                string fkriter = v.ENTER;
                string foperand = string.Empty;
                string foperand_id = string.Empty;
                int ffieldtype = 0;
                int t1 = VGrid.Rows.Count;
                int t2 = 0;

                #region Vgrid row döngüsü
                // Vgrid üzerindeki category sayısı
                for (int i1 = 0; i1 < t1; i1++)
                {
                    // category altındaki edittext sayısı
                    t2 = VGrid.Rows[i1].ChildRows.Count;

                    // t2 = 4 ise sorgu tipi even  (başlangıç, bitiş)
                    // t2 = 2 ise sorgu tipi odd   (tek sorgu)

                    #region categori altı
                    for (int i2 = 0; i2 < t2; i2++)
                    {
                        fullname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();

                        TableIPCode_Get(fullname, ref softCode, ref projectCode, ref ttable_alias, ref fname);

                        fvalue = string.Empty;
                        fvalue2 = string.Empty;
                        foperand = string.Empty;
                        foperand_id = string.Empty;
                        ffieldtype = 0;

                        #region // fieldin tipi nedir, tespiti
                        if (VGrid.Rows[i1].ChildRows[i2].Tag != null)
                            ffieldtype = System.Convert.ToInt32(VGrid.Rows[i1].ChildRows[i2].Tag);

                        // field tipine göre veri hazırlnacak 
                        if (VGrid.Rows[i1].ChildRows[i2].Properties.Value != null)
                        {
                            fvalue = VGrid.Rows[i1].ChildRows[i2].Properties.Value.ToString();

                            //fvalue = t.tData_Convert(fvalue, ffieldtype);
                            // bu ikinci veriye Like lar yüzünden ihtiyaç duyuldu.
                            fvalue2 = t.Str_Check(VGrid.Rows[i1].ChildRows[i2].Properties.Value.ToString());
                        }
                        #endregion

                        #region // Vgridin satırı sorgu çeşitleri değilse ve elle girilmiş veri var  ise

                        if ((fname.IndexOf("bas_sorgu_") == -1) &&
                            (fname.IndexOf("bit_sorgu_") == -1) &&
                            (fvalue != ""))
                        {
                            if (FieldName == fname)
                            {
                                //MessageBox.Show(fullname);
                                if ((default_type == 51) && (i2 == 0))
                                {
                                    tValue = fvalue;
                                    break;
                                }
                                if ((default_type == 52) && (i2 == 1))
                                {
                                    tValue = fvalue;
                                    break;
                                }
                                if ((default_type == 53) && (i2 == 0))
                                {
                                    tValue = fvalue;
                                    break;
                                }
                            }

                            /*
                            // operand tipi belirlenecek
                            if (t2 == 4) // even ise (çift)
                            {
                                if (VGrid.Rows[i1].ChildRows[i2 + 2].Properties.Value != null)
                                    foperand_id = VGrid.Rows[i1].ChildRows[i2 + 2].Properties.Value.ToString();
                            }

                            if (t2 == 2) // odd ise (tek)
                            {
                                if (VGrid.Rows[i1].ChildRows[i2 + 1].Properties.Value != null)
                                    foperand_id = VGrid.Rows[i1].ChildRows[i2 + 1].Properties.Value.ToString();
                            }
                            */
                            /*
                            foperand = Kriter_Operand_Find(foperand_id);
                            //---

                            // parçalar bir araya geldi, şimdi and ile başlayan yeni bir where parçası oluştulacak

                            if (foperand == "%%")
                                tmpkriter = tmpkriter + " and " + fname + " like '%" + fvalue2 + "%' " + v.ENTER;
                            else if (foperand == "%")
                                tmpkriter = tmpkriter + " and " + fname + " like '" + fvalue2 + "%' " + v.ENTER;
                            else if (foperand != string.Empty)
                                tmpkriter = tmpkriter + " and " + fname + " " + foperand + " " + fvalue + v.ENTER;

                            // ------

                            if (t.IsNotNull(tmpkriter))
                            {
                                if (t.IsNotNull(tkrt_alias))
                                {
                                    aSQL = t.SQLWhereAdd(aSQL, tkrt_alias, tmpkriter, "KRITERLER");
                                }
                                else
                                {
                                    fkriter = fkriter + tmpkriter;
                                }

                                tmpkriter = string.Empty;
                            }
                            */
                        }
                        #endregion

                    }
                    #endregion
                }
                #endregion
            }

            return tValue;
        }

        public string Find_TableIPCode_Value(Form tForm, string TableIPCode, string FieldName)
        {
            string tValue = "";

            DataSet dS = null;
            DataNavigator dN = null;
            Find_DataSet(tForm, ref dS, ref dN, TableIPCode);

            /// FieldName gelmediyse KeyField okunacak
            if (FieldName == "" && IsNotNull(dS))
            {
                string myProp_ = dS.Namespace.ToString();
                FieldName = Set(MyProperties_Get(myProp_, "KeyFName:"),"", "");
                if (FieldName == "") return tValue;
            }

            if (IsNotNull(dS) &&
                dN.Position > -1 &&
                IsNotNull(FieldName))
            {
                tValue = dS.Tables[0].Rows[dN.Position][FieldName].ToString();
            }
            if (IsNotNull(dS) &&
                dN.Position == -1 &&
                IsNotNull(FieldName))
            {
                if (dS.Tables[0].Rows.Count > 0)
                    tValue = dS.Tables[0].Rows[0][FieldName].ToString();
            }

            return tValue;
        }

        public string Find_Properies_Get_RowBlock(string thisBlocks, string MainFName, string FindValues)
        {
            /*
            3=ROW_PROP_NAVIGATOR:3;
            3=CAPTION:ccccccc;
     >>     3=BUTTONTYPE:56;
            3=TABLEIPCODE_LIST:null;
            3=FORMNAME:null;
            3=FORMCODE:null;
            3=FORMTYPE:null;
            3=FORMSTATE:null;
            3=ROWE_PROP_NAVIGATOR:3;
            PROP_NAVIGATOR=}
            */

            string s = thisBlocks;
            string find_block = string.Empty;

            if (thisBlocks.IndexOf(FindValues) > -1)
            {
                Get_And_Clear(ref s, FindValues);
                string block = MyProperties_Get(s, "=ROWE_" + MainFName + ":");

                string block_basi = block + "=ROW_" + MainFName;
                string block_sonu = block + "=ROWE_" + MainFName + ":" + block;

                int i1 = thisBlocks.IndexOf(block_basi);
                int i2 = thisBlocks.IndexOf(block_sonu) + block_sonu.Length;

                if ((i1 > -1) && (i2 > i1))
                    find_block = thisBlocks.Substring(i1, i2 - i1);
            }

            return find_block;
        }

        public string Find_Properies_Get_RowBlock(ref string thisBlocks, string MainFName)
        {
            #region  
            /*
            0=J_WHERE:J_WHERE={
            1=ROW_J_WHERE:1;      <<<<< row başı
            1=CAPTION:MS_FIELDS.TABLE_CODE;
            1=J_TABLE_ALIAS:B;
            1=MT_FNAME:TABLE_CODE;
            1=J_FNAME:TABLE_CODE;
            1=FVALUE:null;
            1=ROWE_J_WHERE:1;     <<<<< row sonu
            2=ROW_J_WHERE:2;
            2=CAPTION:MS_FIELDS.FIELD_NO;
            2=J_TABLE_ALIAS:B;
            2=MT_FNAME:FIELD_NO;
            2=J_FNAME:FIELD_NO;
            2=FVALUE:null;
            2=ROWE_J_WHERE:2;
            J_WHERE=};
            */
            #endregion 

            string row_block = string.Empty;

            string row_basi = "=ROW_" + MainFName;
            string row_sonu = "=ROWE_" + MainFName + ":";

            int i1 = 0;
            int i2 = 0;

            if (thisBlocks.IndexOf(row_sonu) > -1)
            {
                i1 = thisBlocks.IndexOf(row_basi);
                i2 = thisBlocks.IndexOf(row_sonu) + row_sonu.Length;

                if ((i1 > -1) && (i2 > i1))
                    row_block = thisBlocks.Substring(i1, i2 - i1);

                thisBlocks = thisBlocks.Remove(i1, i2 - i1);
            }

            return row_block;
        }

        public string Find_Properies_Get_FieldBlock(string thisBlocks, string MainFName)
        {
            #region
            /*
            PROP_JOINTABLE={
            0=ROW_PROP_JOINTABLE:0;           <<<< buradan 
            0=J_TABLE:J_TABLE={
            1=ROW_J_TABLE:1;
            1=CAPTION:MS_FIELDS;
            1=J_FORMAT:STANDART;
            1=J_TYPE:LEFT;
            1=J_TABLE_NAME:MS_FIELDS;
            1=J_TABLE_ALIAS:B;
            1=ROWE_J_TABLE:1;
            J_TABLE=};                        <<<< arası isteniyor
            0=J_WHERE:J_WHERE={
            1=ROW_J_WHERE:1;
            1=CAPTION:MS_FIELDS.TABLE_CODE;
            1=J_TABLE_ALIAS:B;
            1=MT_FNAME:TABLE_CODE;
            1=J_FNAME:TABLE_CODE;
            1=FVALUE:null;
            1=ROWE_J_WHERE:1;
            2=ROW_J_WHERE:2;
            2=CAPTION:MS_FIELDS.FIELD_NO;
            2=J_TABLE_ALIAS:B;
            2=MT_FNAME:FIELD_NO;
            2=J_FNAME:FIELD_NO;
            2=FVALUE:null;
            2=ROWE_J_WHERE:2;
            J_WHERE=};
            0=J_STN_FIELDS:J_STN_FIELDS={
            1=ROW_J_STN_FIELDS:1;
            1=CAPTION:FIELD NAME;
            1=J_TABLE_ALIAS:B;
            1=J_FNAME:FIELD_NAME;
            1=FNL_FNAME:LKP_FIELD_NAME;
            1=ROWE_J_STN_FIELDS:1;
            2=ROW_J_STN_FIELDS:2;
            2=CAPTION:FCAPTION;
            2=J_TABLE_ALIAS:B;
            2=J_FNAME:FCAPTION;
            2=FNL_FNAME:LKP_FCAPTION;
            2=ROWE_J_STN_FIELDS:2;
            J_STN_FIELDS=};
            0=J_CASE_FIELDS:null;
            0=ROWE_PROP_JOINTABLE:0;
            PROP_JOINTABLE=}
                         
             */
            #endregion

            int i1 = 0;
            int i2 = 0;
            string field_block = string.Empty;
            string paket_basi = MainFName + "={";
            string paket_sonu = MainFName + "=};";


            if (thisBlocks.IndexOf(paket_basi) > -1)
            {
                //Str_Remove(ref thisFullBlockValues, paket_basi);
                //Str_Remove(ref thisFullBlockValues, paket_sonu);
                //thisBlocks = thisBlocks.Trim();

                i1 = thisBlocks.IndexOf(paket_basi);
                i2 = thisBlocks.IndexOf(paket_sonu) + paket_sonu.Length;

                if ((i1 > -1) && (i2 > i1))
                    field_block = thisBlocks.Substring(i1, i2 - i1);

            }

            return field_block;
        }

        public string Get_Properties_Value(v.myProperties type, string fullBlocks, string FieldName, ref int Position)
        {
            string values = string.Empty;
            if (Position < 0) Position = 0;

            int i1 = 0;
            int i2 = 0;
            string paket_basi = string.Empty;
            string paket_sonu = string.Empty;

            if (type == v.myProperties.Column)
            {
                values = MyProperties_Get(fullBlocks, FieldName + ":");
                Position = -1;
                return values;
            }

            if (type == v.myProperties.Block)
            {
                paket_basi = FieldName + "={" + v.ENTER;
                paket_sonu = FieldName + "=}";
            }

            if (type == v.myProperties.Row)
            {
                paket_basi = "=ROW_" + FieldName;
                paket_sonu = "=ROWE_" + FieldName + ":";
            }

            i1 = fullBlocks.IndexOf(paket_basi, Position);
            i2 = fullBlocks.IndexOf(paket_sonu, Position);

            if ((i1 > -1) && (i2 > i1))
            {
                Position = i2 + paket_sonu.Length;
                i1 = i1 + paket_basi.Length;
                //i2 = i2 - paket_basi.Length;
                values = fullBlocks.Substring(i1, (i2 - i1));
            }
            else Position = -1;

            return values;
        }
        
        public string TableIPCodeList_Get_Values_OLD(Form tForm, string TableIPCode_RowBlock)
        {
            #region örnek
            /*
             
            1=TABLEIPCODE_LIST:TABLEIPCODE_LIST={   <<<<< field bloğu        <<< okunacak TABLEIPCODE ler
            1=ROW_TABLEIPCODE_LIST:1;               <<<<< row bloğu
            1=CAPTION:Kart aç;
            1=TABLEIPCODE:TSTDTL.TSTDTL_03;
            1=TABLEALIAS:[TSTDTL];
            1=KEYFNAME:MSTR_REF_ID;
            1=RTABLEIPCODE:TSTMSTR.TSTMSTR_01;
            1=RKEYFNAME:REF_ID;
            1=MSETVALUE:null;
            1=WORKTYPE:READ;
            1=ROWE_TABLEIPCODE_LIST:1;
            2=ROW_TABLEIPCODE_LIST:2;
            2=CAPTION:Cari Hesabı Oku;
            2=TABLEIPCODE:HP_CARI.HP_CARI_02;
            2=TABLEALIAS:[HPCARI];
            2=KEYFNAME:REF_ID;
            2=RTABLEIPCODE:TSTMSTR.TSTMSTR_01;
            2=RKEYFNAME:CARI_ID;
            2=MSETVALUE:null;
            2=WORKTYPE:READ;
            2=ROWE_TABLEIPCODE_LIST:2;
            TABLEIPCODE_LIST=};                 <<<<< 
            
            */
            #endregion örnek

            string s = TableIPCode_RowBlock;
            string RTABLEIPCODE = string.Empty;
            string RKEYFNAME = string.Empty;
            string WORKTYPE = string.Empty;
            string ReadValue = string.Empty;

            string row_block = string.Empty;
            string old_row_block = string.Empty;
            string lockE = "=ROWE_";
            string find_row = "MSETVALUE:";

            int i1 = 0;

            while (s.IndexOf(lockE) > -1)
            {
                ReadValue = string.Empty;
                row_block = Find_Properies_Get_RowBlock(ref s, "TABLEIPCODE_LIST");
                old_row_block = row_block;

                RTABLEIPCODE = MyProperties_Get(row_block, "RTABLEIPCODE:");
                RKEYFNAME = MyProperties_Get(row_block, "RKEYFNAME:");
                WORKTYPE = MyProperties_Get(row_block, "WORKTYPE:");

                DataSet ds = null;
                DataNavigator tDataNavigator = null;

                if (WORKTYPE == "NEW")
                {
                    ReadValue = "0";
                }

                if (IsNotNull(RTABLEIPCODE) &&
                    IsNotNull(RKEYFNAME) &&
                    ((WORKTYPE == "READ") ||
                     (WORKTYPE == "SVIEW") ||
                     (WORKTYPE == "SVIEWVALUE") ||
                     (WORKTYPE == "CREATEVIEW") ||
                     (WORKTYPE == "OPENFORM"))
                    )
                {
                    ds = Find_DataSet(tForm, "", RTABLEIPCODE, "");

                    if (IsNotNull(ds))
                    {
                        tDataNavigator = Find_DataNavigator(tForm, RTABLEIPCODE);//, "");

                        if (tDataNavigator != null)
                        {
                            ReadValue = ds.Tables[0].Rows[tDataNavigator.Position][RKEYFNAME].ToString();
                        }
                    }
                }

                if (IsNotNull(ReadValue))
                {
                    i1 = row_block.IndexOf(find_row);
                    i1 = i1 + find_row.Length;

                    if (row_block.IndexOf("MSETVALUE:0") > -1)
                    {
                        row_block = row_block.Remove(i1, 1); // silinecek ifade null dur.   MSETVALUE: > null <;
                        row_block = row_block.Insert(i1, ReadValue);

                        Str_Replace(ref TableIPCode_RowBlock, old_row_block, row_block);
                    }

                    if (row_block.IndexOf("MSETVALUE:null") > -1)
                    {
                        row_block = row_block.Remove(i1, 4); // silinecek ifade null dur.   MSETVALUE: > null <;
                        row_block = row_block.Insert(i1, ReadValue);

                        Str_Replace(ref TableIPCode_RowBlock, old_row_block, row_block);
                    }
                }
            }

            return TableIPCode_RowBlock;
        }

        public void Find_Button_AddClick(Form tForm, string menuName, string buttonName,
                                         DevExpress.XtraBars.ItemClickEventHandler myClick)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, menuName, "", controls);

            if (c != null)
            {
                if (c.GetType().ToString() == "DevExpress.XtraBars.Ribbon.RibbonControl")
                {
                    int i2 = ((DevExpress.XtraBars.Ribbon.RibbonControl)c).Items.Count;
                    for (int i = 0; i < i2; i++)
                    {
                        if ((((DevExpress.XtraBars.Ribbon.RibbonControl)c).Items[i].Name.ToString() == buttonName) ||
                             (buttonName == "ALLBUTTONS"))
                        {
                            ((DevExpress.XtraBars.Ribbon.RibbonControl)c).Items[i].ItemClick += null;
                            ((DevExpress.XtraBars.Ribbon.RibbonControl)c).Items[i].ItemClick += myClick;

                            if (buttonName != "ALLBUTTONS") break;
                        }
                    }
                }
            }
        }

        public void Find_Button_AddClick(Form tForm, string menuName, string buttonName,
                                         DevExpress.XtraBars.Navigation.NavElementClickEventHandler myClick)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, menuName, "", controls);

            if (c != null)
            {
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavPane")
                {
                    DevExpress.XtraBars.Navigation.TileNavPane tnPane = null;
                    tnPane = c as DevExpress.XtraBars.Navigation.TileNavPane;
                    string bname = string.Empty;
                    string iname = string.Empty;

                    int i3 = tnPane.Buttons.Count;
                    int i5 = 0;
                    for (int i2 = 0; i2 < i3; i2++)
                    {
                        bname = tnPane.Buttons[i2].Element.Name.ToString();
                        if (bname == buttonName)
                        {
                            tnPane.Buttons[i2].Element.ElementClick += null;
                            tnPane.Buttons[i2].Element.ElementClick += myClick;
                            break;
                        }

                        /// category / sub menu altındakiler
                        ///
                        i5 = 0;
                        if (tnPane.Buttons[i2].Element.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavCategory")
                        {
                            i5 = ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items.Count;

                            for (int i4 = 0; i4 < i5; i4++)
                            {                                
                                iname = ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items[i4].Name.ToString();
                                if (iname == buttonName)
                                {
                                    ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items[i4].ElementClick += null;
                                    ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items[i4].ElementClick += myClick;
                                    break;
                                }
                            }
                        }
                    }

                }
            }
        }

        public void Find_NavButton_Control(Form tForm, string menuName, string buttonName, ref DevExpress.XtraBars.Navigation.NavElement control_)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, menuName, "", controls);

            if (c != null)
            {
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavPane")
                {
                    DevExpress.XtraBars.Navigation.TileNavPane tnPane = null;
                    tnPane = c as DevExpress.XtraBars.Navigation.TileNavPane;
                    string bname = string.Empty;
                    string iname = string.Empty;

                    int i3 = tnPane.Buttons.Count;
                    int i5 = 0;
                    for (int i2 = 0; i2 < i3; i2++)
                    {
                        bname = tnPane.Buttons[i2].Element.Name.ToString();
                        if (bname == buttonName)
                        {
                            control_ = tnPane.Buttons[i2].Element;
                            break;
                        }

                        /// category / sub menu altındakiler
                        ///
                        i5 = 0;
                        if (tnPane.Buttons[i2].Element.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavCategory")
                        {
                            i5 = ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items.Count;

                            for (int i4 = 0; i4 < i5; i4++)
                            {
                                iname = ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items[i4].Name.ToString();
                                if (iname == buttonName)
                                {
                                    control_ = ((DevExpress.XtraBars.Navigation.TileNavCategory)tnPane.Buttons[i2].Element).Items[i4];
                                    break;
                                }
                            }
                        }
                    }

                }
            }
        }



        public void Find_Button_AddClick_(Form tForm, string menuName, string buttonName,
                                          System.EventHandler myClick)
        {
            string[] controls = new string[] { };
            Control c = Find_Control(tForm, menuName, "", controls);

            if (c != null)
            {
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.AccordionControl")
                {
                    DevExpress.XtraBars.Navigation.AccordionControl mControl = null;
                    mControl = c as DevExpress.XtraBars.Navigation.AccordionControl;
                    string bname = string.Empty;
                    string iname = string.Empty;

                    int i3 = mControl.Elements.Count;
                    int i5 = 0;
                    for (int i2 = 0; i2 < i3; i2++)
                    {
                        bname = mControl.Elements[i2].Name.ToString();
                        if (bname == buttonName)
                        {
                            mControl.Elements[i2].Click += null;
                            mControl.Elements[i2].Click += myClick;
                            break;
                        }

                        i5 = mControl.Elements[i2].Elements.Count;
                        for (int i4 = 0; i4 < i5; i4++)
                        {
                            bname = mControl.Elements[i2].Elements[i4].Name.ToString();
                            if (bname == buttonName)
                            {
                                mControl.Elements[i2].Elements[i4].Click += null;
                                mControl.Elements[i2].Elements[i4].Click += myClick;
                                break;
                            }
                        }
                    }
                }
            }
        }
                
        public string Find_Properties_Value(string SelectBlockValue, string tRowFName)
        {
            string tValue = string.Empty;
            string s = SelectBlockValue;
            int j1 = -1;
            int j2 = -1;

            if (tRowFName == "TLP")
            {
                // iki properties içinde TLP mevcut
                // bu nedenle hangisi geldi onu bulmak gerekiyor
                int i = SelectBlockValue.IndexOf("DOCKPANEL");
                if (i >= 0)
                {
                    /*
                    PROP_VIEWS_LAYOUT packet = new PROP_VIEWS_LAYOUT();
                    SelectBlockValue = SelectBlockValue.Replace((char)34, (char)39);
                    //var prop_ = JsonConvert.DeserializeAnonymousType(SelectBlockValue, packet);
                    */
                    PROP_VIEWS_LAYOUT prop_ = readProp<PROP_VIEWS_LAYOUT>(SelectBlockValue);

                    tValue = JsonConvert.SerializeObject(prop_.TLP, Newtonsoft.Json.Formatting.Indented);
                    return tValue;
                }
                if (i == -1)
                {
                    /*
                    PROP_VIEWS_GROUPS packet = new PROP_VIEWS_GROUPS();
                    SelectBlockValue = SelectBlockValue.Replace((char)34, (char)39);
                    //var prop_ = JsonConvert.DeserializeAnonymousType(SelectBlockValue, packet);
                    */
                    PROP_VIEWS_GROUPS prop_ = readProp<PROP_VIEWS_GROUPS>(SelectBlockValue);
                    tValue = JsonConvert.SerializeObject(prop_.TLP, Newtonsoft.Json.Formatting.Indented);
                    return tValue;
                }
            }

            /// "SV_LIST": [
            string paket_basi_A = (char)34 + tRowFName + (char)34 + ": [";
            string paket_sonu_A = "]";

            /// "SV_LIST": {
            string paket_basi_B = (char)34 + tRowFName + (char)34 + ": {";
            string paket_basi_B2 = (char)34 + tRowFName + (char)34 + ": ";
            string paket_sonu_B = "}";

            /// başka bir blok, paket varsa
            /// "SV_LIST": [
            j1 = s.IndexOf(paket_basi_A);
            if (j1 > -1)
            {
                j2 = s.IndexOf(paket_sonu_A, j1) + paket_sonu_A.Length;
                if (j2 > j1)
                {
                    tValue = s.Substring(j1, j2 - j1);
                    tValue = tValue.Trim();
                }
            }

            /// başka bir blok, paket varsa
            /// "SV_LIST": {
            if (j1 == -1)
            {
                j1 = s.IndexOf(paket_basi_B);
                if (j1 > -1)
                {
                    j1 = j1 + paket_basi_B2.Length;
                    j2 = s.IndexOf(paket_sonu_B, j1) + paket_sonu_B.Length;
                    tValue = s.Substring(j1, j2 - j1);
                    tValue = tValue.Trim();
                }
            }

            if (j1 == -1)
            {
                /// sadece fname ve value değeri var ise
                /// "SV_ENABLED": "TRUE"
                tValue = MyProperties_Get(s, (char)34 + tRowFName + (char)34 + ": " + (char)34); //"
                //tValue = t.MyProperties_Get(s, (char)39 + tRowFName + (char)39 + ": " + (char)39); //'
                tValue = tValue.Trim();
                if (tValue.IndexOf("null") == 0)
                    tValue = "";
            }

            return tValue;
        }

        public string Find_External_TableIPCode(DataSet dsData)
        {
            //=External_TableIPCode:UST/MEB/MtskAday.Kayit_F02;
            string tableIPCode = "";
            string myProp = dsData.Namespace.ToString();
            if (myProp.IndexOf("External_IP:True") > -1)
            {
                tableIPCode = MyProperties_Get(myProp, "External_TableIPCode:");
            }
            return tableIPCode;
        }

        public int Find_TabControlPageIndex(Control c)
        {
            int pageNo = 0;

            if (c != null)
            {
                #region XtraTab.XtraTabControl
                if (c.GetType().ToString() == "DevExpress.XtraTab.XtraTabControl")
                {
                    pageNo = ((DevExpress.XtraTab.XtraTabControl)c).SelectedTabPageIndex;
                }
                #endregion

                #region Navigation.NavigationPane
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
                {
                    pageNo = ((DevExpress.XtraBars.Navigation.NavigationPane)c).SelectedPageIndex;
                }
                #endregion

                #region Navigation.TabPane
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane")
                {
                    pageNo = ((DevExpress.XtraBars.Navigation.TabPane)c).SelectedPageIndex;
                }
                #endregion
            }

            return pageNo;
        }

        public bool Find_FieldName(DataSet ds, string findFieldName)
        {
            bool onay = false;
            string fName = "";
            int colCount = ds.Tables[0].Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                fName = ds.Tables[0].Columns[i].ColumnName;
                if (fName == findFieldName)
                {
                    onay = true;
                    break;
                }
            }
            return onay;
        }

        public string getPropNavigator(Form tForm, string tableIPCode)
        {
            tToolBox t = new tToolBox();
            string value = "";
            if (tForm == null) return value;
            if (t.IsNotNull(tableIPCode) == false) return value;

            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, tableIPCode);
            if (cntrl == null) return value;

            if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
            {
                if (((GridControl)cntrl).AccessibleDescription != null)
                {
                    value = ((GridControl)cntrl).AccessibleDescription.ToString();
                    //return value;
                }
            }

            if (cntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
            {
                if (((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription != null)
                {
                    value = ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription.ToString();
                    //return value;
                }
            }
            
            Str_Remove(ref value, "|ds|");
            value = value.Trim();
            return value;
        }


        #endregion Diğerleri <<

        #endregion Find Functions

        #region *Tarih Fonksiyonları

        #region Tarih_Formati

        public string Tarih_Formati(DateTime Tarih)
        {
            string s = "";
            string t = "";
            if (Tarih.Year > 0)
            {
                t = Tarih.Date.ToString().Substring(0, 10);
                                
                if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                    s = " Convert(Date, '" + t + "', 103) ";
            }
            else s = "null";
            return s;
        }

        public string TarihSaat_Formati(DateTime Tarih)
        {
            string s = "";
            string t = "";
            string saat = "";

            if (Tarih.Year > 0)
            {
                t = Tarih.ToString();
                saat = Tarih.ToLongTimeString();
                
                if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                {
                    //s = "'" + Tarih.ToString("MM.dd.yyyy", CultureInfo.InvariantCulture) + " " + saat + "'";
                    // CONVERT(SMALLDATETIME, '05.29.2015 00:00:00', 101)
                    //s = " Convert(SmallDatetime, " + s + ", 101) ";

                    s = Tarih.Date.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture).Substring(0, 10) + ' ' + Tarih.TimeOfDay.ToString().Substring(0, 8);
                    s = " Convert(Datetime, '" + s + "', 101) ";
                }
            }
            else s = "null";

            return s;
        }

        public string TarihSaat_Formati_End(DateTime Tarih)
        {
            string s = "";
            string t = "";
            string saat = "";

            if (Tarih.Year > 0)
            {
                t = Tarih.ToString();

                saat = "23:59:00";

                //s = "'" +
                //    t[3].ToString() + t[4].ToString() + "." +
                //    t[0].ToString() + t[1].ToString() + "." +
                //    t[6].ToString() + t[7].ToString() +
                //    t[8].ToString() + t[9].ToString() + " " + saat + "'";

                s = "'" + Tarih.ToString("MM.dd.yyyy", CultureInfo.InvariantCulture) + " " + saat + "'";


            }
            else s = "null";
            return s;
        }

        public DateTime Tarih_Set(string Read_Date)
        {
            DateTime Tarih = Convert.ToDateTime("01.01.0001");
            try
            {
                Tarih = Convert.ToDateTime(Read_Date);
            }
            catch (Exception)
            {
                Tarih = Convert.ToDateTime("01.01.0001");
            }

            return Tarih;
        }

        public string TarihSaat_Etiketi_Preparing()
        {
            DateTime Tarih = DateTime.Now;
            string s = Tarih.ToLongTimeString();

            string n = "";
            string t = "";

            if (Tarih.Year > 0)
            {
                t = Tarih.ToString();

                n = t[8].ToString() + t[9].ToString() + // yıl
                    t[3].ToString() + t[4].ToString() + // ay
                    t[0].ToString() + t[1].ToString() + '_' +// gun
                    s[0].ToString() + s[1].ToString() + // hh : saat
                    s[3].ToString() + s[4].ToString() + // dd :
                    s[6].ToString() + s[6].ToString();

            }
            else n = "null";
            return n;
        }

        public string Formatli_TarihSaat_yyyyMMdd_hhmm()
        {
            //13.07.2025 13:33:58
            string tarih_ = DateTime.Now.ToString();
            tarih_ = tarih_.Replace(".", "");
            tarih_ = tarih_.Replace(" ", "_");
            tarih_ = tarih_.Replace(":", "");
            //13072025_133358
            string gun_ = tarih_.Substring(0, 2);
            string ay_ = tarih_.Substring(2, 2);
            string yil_ = tarih_.Substring(4, 4);
            string saat_ = tarih_.Substring(8, 5);
            tarih_ = yil_ + ay_ + gun_ + saat_;

            return tarih_;
        }

        #endregion

        #region Tarih Aralığı

        public void Tarih_Araligi_Haftalik(DateTime MasterTarih, ref DateTime BasTarih, ref DateTime BitTarih, int ArtiHafta, int KacHafta)
        {
            if (BasTarih.ToString() == "01.01.0001 00:00:00")
            {
                BasTarih = v.BU_HAFTA_BASI_TARIH.AddDays(ArtiHafta * 7);
                BitTarih = v.BU_HAFTA_BASI_TARIH.AddDays((ArtiHafta * 7) + ((KacHafta * 7) - 1));
            }
            else
            {
                BasTarih = MasterTarih.AddDays(ArtiHafta * 7);
                BitTarih = MasterTarih.AddDays((ArtiHafta * 7) + ((KacHafta * 7) - 1));
            }
        }

        public int Iki_Tarih_Arasi_Kac_Gun(DateTime Bas_Tarih, DateTime Bit_Tarih)
        {
            System.TimeSpan zaman;
            zaman = Bit_Tarih.Subtract(Bas_Tarih);
            int fark = Convert.ToInt32(zaman.TotalDays);
            return fark;
        }

        public void Tarihin_Ayin_IlkGunu_SonGunu(DateTime Tarih, ref DateTime IlkGun, ref DateTime SonGunu)
        {
            IlkGun = new DateTime(Tarih.Year, Tarih.Month, 1);
            SonGunu = IlkGun.AddMonths(1).AddDays(-1);
        }

        #endregion Tarih Aralığı

        #endregion *Tarih Fonksiyonları

        #region *mySoru

        public DialogResult mySoru(string Soru)
        {
            string Question = Soru;

            if (Soru.ToUpper() == "VAZGEÇ")
                Question = "İşlemden vazgeçmek ister misiniz?";

            if ((Soru.ToUpper() == "DEVAM") ||
                (Soru.ToUpper() == "ONAY") ||
                (Soru.ToUpper() == "SİL") ||
                (Soru.ToUpper() == "SIL"))
                Question = "İşleme devam etmek istediğinize emin misiniz?";

            if (Soru.ToUpper() == "EXIT")
                Question = "Program kapatılacak, kapatmak istediğinize emin misiniz?";


            DialogResult answer = MessageBox.Show(Question, "Onay İşlemi", MessageBoxButtons.YesNoCancel);

            switch (answer)
            {
                case DialogResult.Yes:
                    {
                        break; // break ifadesini sakın silme
                    }
                case DialogResult.No:
                    {
                        break; // break ifadesini sakın silme
                    }
                case DialogResult.Cancel:
                    {
                        break; // break ifadesini sakın silme
                    }
            }

            return answer;
        }


        #endregion mySoru

        #region *Grid Fonksiyonları

        public void GridGroupRefresh(Control cntrl)
        {
            GridControl grid = null;
            if (cntrl != null)
            {
                grid = (GridControl)cntrl;
            }
            if (grid != null)
            {
                string gType = grid.MainView.GetType().ToString();
                if ((gType == "DevExpress.XtraGrid.Views.Grid.GridView") ||
                    (gType == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView"))
                {
                    string GROUPFNAME1 = string.Empty;
                    string GROUPFNAME2 = string.Empty;
                    string GROUPFNAME3 = string.Empty;

                    int i1 = ((GridView)grid.MainView).GroupedColumns.Count;
                    if (i1 > 0) GROUPFNAME1 = ((GridView)grid.MainView).GroupedColumns.View.GroupedColumns[0].FieldName;
                    if (i1 > 1) GROUPFNAME2 = ((GridView)grid.MainView).GroupedColumns.View.GroupedColumns[1].FieldName;
                    if (i1 > 2) GROUPFNAME3 = ((GridView)grid.MainView).GroupedColumns.View.GroupedColumns[2].FieldName;

                    if (IsNotNull(GROUPFNAME3))
                        ((GridView)grid.MainView).Columns[GROUPFNAME3].GroupIndex = -1;
                    if (IsNotNull(GROUPFNAME2))
                        ((GridView)grid.MainView).Columns[GROUPFNAME2].GroupIndex = -1;
                    if (IsNotNull(GROUPFNAME1))
                        ((GridView)grid.MainView).Columns[GROUPFNAME1].GroupIndex = -1;

                    if (IsNotNull(GROUPFNAME1))
                        ((GridView)grid.MainView).Columns[GROUPFNAME1].GroupIndex = 0;
                    if (IsNotNull(GROUPFNAME2))
                        ((GridView)grid.MainView).Columns[GROUPFNAME2].GroupIndex = 1;
                    if (IsNotNull(GROUPFNAME3))
                        ((GridView)grid.MainView).Columns[GROUPFNAME3].GroupIndex = 2;
                }
            }
        }

        public void Gridi_Grupla(GridControl grid,
                                 string Group0_Name, string Group1_Name, string Group2_Name,
                                 ref string Group0_Old, ref string Group1_Old, ref string Group2_Old)
        {
            if (grid != null)
            {
                if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                {
                    //GridView tGridView = grid.MainView as GridView;
                    //GridView tGridView = (DevExpress.XtraGrid.Views.Grid.GridView)grid.MainView;
                    //GridView tGridView = new GridView(grid);

                    if (IsNotNull(Group0_Old))
                        ((GridView)grid.MainView).Columns[Group0_Old].GroupIndex = -1;
                    if (IsNotNull(Group1_Old))
                        ((GridView)grid.MainView).Columns[Group1_Old].GroupIndex = -1;
                    if (IsNotNull(Group2_Old))
                        ((GridView)grid.MainView).Columns[Group2_Old].GroupIndex = -1;

                    if (IsNotNull(Group0_Name))
                        ((GridView)grid.MainView).Columns[Group0_Name].GroupIndex = 0;
                    if (IsNotNull(Group1_Name))
                        ((GridView)grid.MainView).Columns[Group1_Name].GroupIndex = 1;
                    if (IsNotNull(Group2_Name))
                        ((GridView)grid.MainView).Columns[Group2_Name].GroupIndex = 2;

                    Group0_Old = Group0_Name;
                    Group1_Old = Group1_Name;
                    Group2_Old = Group2_Name;

                    ((GridView)grid.MainView).ExpandAllGroups();
                }
            }
        }

        #endregion Grid Fonksiyonları

        #region *InputBox
        //public DialogResult InputBox(string title, string promptText, ref string value, int ftype, string displayFormat)
        public DialogResult UserInpuBox(vUserInputBox vUIBox)
        {
            Form form = new DevExpress.XtraEditors.XtraForm();//new Form();
            Label label = new Label();
            
            //TextBox textBox = new TextBox();
            
            DevExpress.XtraEditors.TextEdit textBox = new DevExpress.XtraEditors.TextEdit();
            
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            
            form.Font = new Font("Tahoma", 10);
            form.Text = vUIBox.title;       // title;
            label.Text = vUIBox.promptText; // promptText;
            
            buttonOk.Text = "Tamam";
            buttonCancel.Text = "Vazgeç";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 26);
            label.Font = new Font("Tahoma", 10);

            textBox.SetBounds(12, 80, 372, 20);
            textBox.Font = new Font("Tahoma", 22);

            if ((vUIBox.displayFormat != "") ||
                (vUIBox.displayFormat != null))
            {
                // Password tanımı ise
                if (vUIBox.displayFormat == "*")
                {
                    textBox.Properties.PasswordChar = '*';
                }
                else
                {
                    textBox.Properties.Mask.EditMask = vUIBox.displayFormat;
                    textBox.Properties.Mask.SaveLiteral = true;
                    textBox.Properties.Mask.ShowPlaceHolders = true;
                    textBox.Properties.Mask.UseMaskAsDisplayFormat = true;
                }
            }

            // set value             
            textBox.Text = vUIBox.value;   

            /// if ((tcmp_format_type == 0) || (tcmp_format_type == 1))
            ///     {
            ///         tEdit.RightToLeft = RightToLeft.Yes;
            ///         tEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            ///     }
            ///     if (tcmp_format_type == 2) tEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.DateTime;
            ///     if (tcmp_format_type == 3) tEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Custom;
            /// 
            ///     tEdit.Properties.Mask.EditMask = tdisplayformat;
            ///     tEdit.Properties.Mask.SaveLiteral = true;
            ///     tEdit.Properties.Mask.ShowPlaceHolders = true;
            ///     tEdit.Properties.Mask.UseMaskAsDisplayFormat = true;

            //* date 40
            if ((vUIBox.fieldType == 40) |
                (vUIBox.fieldType == 61))
            {
                textBox.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.DateTime;
            }

            //* rakam  56, 48, 127, 52, 60, 62, 59, 106, 108
            if ((vUIBox.fieldType == 56) | (vUIBox.fieldType == 48) |
                (vUIBox.fieldType == 59) | (vUIBox.fieldType == 52) |
                (vUIBox.fieldType == 60) | (vUIBox.fieldType == 62) |
                (vUIBox.fieldType == 127) | (vUIBox.fieldType == 106) |
                (vUIBox.fieldType == 108))
            {
                textBox.Text = "";
                textBox.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far; 
                textBox.Properties.Appearance.Options.UseTextOptions = true;
                textBox.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
                textBox.Properties.Mask.EditMask = "n";
                textBox.Properties.Mask.AutoComplete = DevExpress.XtraEditors.Mask.AutoCompleteType.Default;
                textBox.Properties.Mask.PlaceHolder = '.';
                textBox.Text = vUIBox.value;
                if (vUIBox.value == "") textBox.Text = "0";
            }

            //buttonOk.SetBounds(228, 72, 75, 23);
            //buttonCancel.SetBounds(309, 72, 75, 23);
            buttonOk.SetBounds(228, 128, 75, 25);
            buttonCancel.SetBounds(309, 128, 75, 25);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 175);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(400, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();

            vUIBox.value = textBox.Text;

            return dialogResult;
        }
        #endregion InputBox

        #region Report.Select/TableIPCODE işlemleri
        
        public void Preparing_Select_ReportData_DataSet(Form tForm,
                    ref DataSet dsData,
                    byte DBaseNo, string TableIPCode, string Kriterler)
        {
            if (dsData.Namespace.ToString() == "")
            {
                DataSet ds_Table = new DataSet();
                DataSet ds_Fields = new DataSet();

                tTablesRead tr = new tTablesRead();

                tr.MS_Tables_IP_Read(ds_Table, TableIPCode);
                tr.MS_Fields_IP_Read(ds_Fields, TableIPCode);

                if ((IsNotNull(ds_Table) == false) ||
                    (IsNotNull(ds_Fields) == false)) return;

                DataRow row_Table = ds_Table.Tables[0].Rows[0] as DataRow;

                vTableAbout vTA = new vTableAbout();
                Table_About(vTA, ds_Fields);

                tSQLs sql = new tSQLs();
                sql.Preparing_dsData(tForm, row_Table, ds_Fields, ref dsData, "", vTA);
            }

            string myProp = dsData.Namespace.ToString();
            string tTableName = MyProperties_Get(myProp, "TableName:");
            string tTableCaption = MyProperties_Get(myProp, "TableCaption:");

            string SqlF = MyProperties_Get(myProp, "SqlFirst:");
            //string SqlS = MyProperties_Get(myProp, "SqlSecond:");

            string block = string.Empty;
            string and_block = string.Empty;
            string alias = string.Empty;

            if (Kriterler != null)
            {
                while (Kriterler.IndexOf("=ROWE_") > -1)
                {
                    block = Get_And_Clear(ref Kriterler, "=ROWE_");
                    alias = MyProperties_Get(block, "ALIAS:");
                    and_block = MyProperties_Get(block, "_PARAMS:");

                    if (and_block.Length > 0)
                    {
                        tKriter_Ekle(ref SqlF, alias, and_block);
                    }
                }
            }
            /*
            =ROW_[VRSNL_10]:null;
            =[VRSNL_10]_PARAMS: and [VRSNL_10].PART_NAME like  '%aaa%' 
             and [VRSNL_10].BAS_TARIH =   CONVERT(SMALLDATETIME, '01.01.2015 00:00:00', 101)  
             and [VRSNL_10].GIRIS_YOK = 2
             and [VRSNL_10].GIRIS_YAPTI = 3
            ;
            =ROWE_[VRSNL_10]:null;
            */

            if (Kriterler != "")
            {
                try
                {
                    SqlF = kisitlamalarClear(SqlF);
                    Data_Read_Execute(tForm, dsData, ref SqlF, tTableName, null);
                    dsData.Tables[tTableName].Namespace = tTableCaption;

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                }
            }

        }

        public void Preparing_Select_Param_DataSet(Form tForm,
                    ref DataSet dsKrtr,
                    byte DBaseNo, string TableIPCode)
        {
            tSQLs sql = new tSQLs();

            string sqlKrtr_Kist = string.Empty;
            string sqlKrtr_Full = string.Empty;
            string tTOP_Count = string.Empty;

            DataSet ds_Table = new DataSet();

            tTablesRead tr = new tTablesRead();

            tr.MS_Tables_IP_Read(ds_Table, TableIPCode); //TableIPCode

            string tTableCaption = ds_Table.Tables[0].Rows[0]["IP_CAPTION"].ToString();
            string tTableCode = ds_Table.Tables[0].Rows[0]["TABLE_CODE"].ToString();
            string tTableName = ds_Table.Tables[0].Rows[0]["LKP_TABLE_NAME"].ToString();
            //string tKey_FName = ds_Table.Tables[0].Rows[0]["KEY_FNAME"].ToString();

            // tTableType = 6  Select / TableIPCode  ise
            sql.SQL_MSFieldsIPKrtr(TableIPCode, ref sqlKrtr_Kist, ref sqlKrtr_Full);

            try
            {
                Data_Read_Execute(tForm, dsKrtr, ref sqlKrtr_Kist, tTableName + "_KIST", null);
                Data_Read_Execute(tForm, dsKrtr, ref sqlKrtr_Full, tTableName + "_FULL", null);
                dsKrtr.Tables[tTableName + "_KIST"].Namespace = tTableCaption;
                dsKrtr.Tables[tTableName + "_FULL"].Namespace = tTableCaption;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }


        #endregion Report.Select/TableIPCODE işlemleri

        #region Report.Table işlemleri

        public void Preparing_Param_DataSet(Form tForm,
                    ref DataSet dsKrtr,
                    byte DBaseNo,
                    string TableName,
                    string KonuOlanTableCode,
                    string KonuOlanFieldName,
                    string KonuOlanValue,
                    string KonuOlanaAit_Liste_TableIPCode)
        {

            string myProp = string.Empty;
            string DBaseName = Find_dBLongName(DBaseNo.ToString());

            MyProperties_Set(ref myProp, "DBaseNo", DBaseNo.ToString());
            MyProperties_Set(ref myProp, "TableName", TableName);
            MyProperties_Set(ref myProp, "Cargo", "param"); // cargo = data, param, report

            DataSet dsData = null;

            // Form tForm işi şimdilik askıya alındı, form bir defa oluşsun bir tekrar create etmeye
            // kalmasın diye düşünüldü bir an için ama ????

            //dsKrtr = Find_dsParam(tForm, TableName);_

            dsKrtr = new DataSet();
            dsKrtr.Namespace = myProp;

            ReportOrParam_Preparing_DataSet(tForm,
                ref dsData, ref dsKrtr,
                DBaseName, TableName,
                KonuOlanTableCode, KonuOlanFieldName, KonuOlanValue,
                KonuOlanaAit_Liste_TableIPCode, "", true);

        }

        public void Preparing_ReportData(Form tForm,
                    ref DataSet dsData,
                    byte DBaseNo,
                    string TableName,
                    string KonuOlanTableCode,
                    string KonuOlanFieldName,
                    string KonuOlanValue,
                    string KonuOlanaAit_Liste_TableIPCode,
                    string UserParam)
        {

            string myProp = string.Empty;
            string DBaseName = Find_dBLongName(DBaseNo.ToString());

            MyProperties_Set(ref myProp, "DBaseNo", DBaseNo.ToString());
            MyProperties_Set(ref myProp, "TableName", TableName);
            MyProperties_Set(ref myProp, "Cargo", "report"); // cargo = data, param, report


            dsData = new DataSet();
            DataSet dsKrtr = null;

            dsData.Namespace = myProp;

            ReportOrParam_Preparing_DataSet(tForm,
                ref dsData, ref dsKrtr,
                DBaseName, TableName,
                KonuOlanTableCode, KonuOlanFieldName, KonuOlanValue,
                KonuOlanaAit_Liste_TableIPCode, UserParam, false);

            // Firma Tablosu Okunuyor
            ReportOrParam_Preparing_DataSet(tForm,
                ref dsData, ref dsKrtr,
                DBaseName, v.SP_FIRM_TABLENAME,
                KonuOlanTableCode, KonuOlanFieldName, v.SP_FIRM_ID.ToString(),
                KonuOlanaAit_Liste_TableIPCode, UserParam, false);


        }


        public void ReportOrParam_Preparing_DataSet(Form tForm,
                            ref DataSet dsData, ref DataSet dsKrtr,
                            string DBName, string TableName,
                            string KonuOlanTableCode,
                            string KonuOlanFieldName,
                            string KonuOlanValue,
                            string KonuOlanaAit_Liste_TableIPCode,
                            string UserParam, Boolean tParam)
        {
            tSQLs sql = new tSQLs();

            // Burada MS_TABLES e bakarak Master_Table_Name sayesinde birbirine bağlı olan tablolar tespit ediliyor
            // burada hem dsData hemde dsKrtr hazırlanıyor 
            // fakat farklı zamanlarda

            string tBasamak = string.Empty;
            string tTableCaption = string.Empty;
            string tTableCode = string.Empty;
            string uTableCode = string.Empty;
            string tTableName = string.Empty;
            string tKey_FName = string.Empty;
            string tMaster_TableName = string.Empty;
            string tMaster_Key_FName = string.Empty;
            string tForeing_FName = string.Empty;
            string and_in = string.Empty;
            string and_select = string.Empty;
            string in__select = string.Empty;
            string and_full = string.Empty;

            string tFrom = string.Empty;
            string tWhereKeys = string.Empty;
            string KOA_Liste_KonuOlanValue = string.Empty;

            int TableNo = 0;

            // Value
            string s = sql.RSQL_MasterTable(DBName, TableName);

            DataSet dsMSTables = new DataSet();

            try
            {
                Data_Read_Execute(tForm, dsMSTables, ref s, TableName, null);
            }
            catch (Exception)
            {
                //
                throw;
            }

            if (IsNotNull(dsMSTables))
            {

                if (TableName != v.SP_FIRM_TABLENAME)
                {
                    TableRemove(dsData);
                }

                // master ise
                // grp      // and [GRP].ID in ( 
                // grp      //    select [GRP].ID from GRUP [GRP] where 0 = 0   
                // detail ise
                // krsyr >> // and [KRSYR].GRUP_ID in ( 
                // krsyr    //    select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0


                #region // yani Rapor Datası okunacaksa extra alttaki bu işlemler yapılacak
                if (tParam == false)
                {
                    //and_full =

                    MyProperties_Set(ref and_full, "and_full", "BEGIN");

                    foreach (DataRow MSTable_Row in dsMSTables.Tables[0].Rows)
                    {
                        //basamak ile tablanun kaçıncı union katmanından geldiğini tespit ediyoruz

                        tBasamak = MSTable_Row["BASAMAK"].ToString();
                        tTableCaption = MSTable_Row["TB_CAPTION"].ToString();
                        tTableCode = MSTable_Row["TABLE_CODE"].ToString();
                        tTableName = MSTable_Row["TABLE_NAME"].ToString();
                        tKey_FName = MSTable_Row["KEY_FNAME"].ToString();
                        tMaster_TableName = MSTable_Row["MASTER_TABLE_NAME"].ToString();
                        tMaster_Key_FName = MSTable_Row["MASTER_KEY_FNAME"].ToString();
                        tForeing_FName = MSTable_Row["FOREING_FNAME"].ToString();

                        if (TableNo == 0)
                        {
                            and_in = " and [" + tTableCode + "]." + tKey_FName + " in ( ";
                            and_select = " select [" + tTableCode + "]." + tKey_FName + " from " + tTableName + " [" + tTableCode + "] where 0 = 0 ";
                            in__select = "";
                            //if (IsNotNull(KonuOlanValue))
                            //    tWhereKeys = " [" + uTableCode + "]." + tKey_FName + " = " + KonuOlanValue + " ";
                        }
                        else
                        {
                            and_in = " and [" + tTableCode + "]." + tForeing_FName + " in ( ";
                            and_select = " select [" + tTableCode + "]." + tForeing_FName + " from " + tTableName + " [" + tTableCode + "] where 0 = 0 ";
                            //in__select = " select [" + tTableCode + "]." + tKey_FName + " from " + tTableName + " [" + tTableCode + "] where [" + tTableCode + "]." + tForeing_FName + " = ";
                            in__select = Preparing_in_select(dsMSTables, tMaster_TableName);
                            //=KRSYR_and_in: and [KRSYR].GRUP_ID in ( ;
                            //=KRSYR_and_select: select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0 ;
                            //=KRSYR_in__select: Select [KRSYR].ID      from KURSIYER [KRSYR] where [KRSYR].GRUP_ID  = ;

                        }

                        MyProperties_Set(ref and_full, tTableCode + "_basamak", tBasamak);
                        MyProperties_Set(ref and_full, tTableCode + "_and_in", and_in);
                        MyProperties_Set(ref and_full, tTableCode + "_and_select", and_select);
                        MyProperties_Set(ref and_full, tTableCode + "_in__select", in__select);


                        TableNo++;
                    }

                    MyProperties_Set(ref and_full, "and_full", "END");


                    if (IsNotNull(UserParam))
                        UserParam = and_full + UserParam;
                    else UserParam = and_full;

                }
                #endregion

                // --------------------------------------------------------------
                // bu döngü hem params lar, hemde Okunacak Data için gerekli
                // eğer dsData null değilse dsKrtr null dur
                // eğer dsKrtr null değilse dsData null dur
                // ikisi aynı anda gelmiyor

                // listedeki datanın ıdsi
                if (IsNotNull(KonuOlanaAit_Liste_TableIPCode) &&
                     IsNotNull(KOA_Liste_KonuOlanValue) == false)
                {
                    DataSet dsKonuOlanList = null;
                    DataNavigator tDataNavigator_KonuOlanList = null;

                    dsKonuOlanList = Find_DataSet(tForm, "", KonuOlanaAit_Liste_TableIPCode, "");
                    tDataNavigator_KonuOlanList = Find_DataNavigator(tForm, KonuOlanaAit_Liste_TableIPCode);//, "");

                    if (tDataNavigator_KonuOlanList != null)
                    {
                        if (tDataNavigator_KonuOlanList.Position > -1)
                        {
                            string myProp = dsKonuOlanList.Namespace.ToString();
                            string read_tablename = MyProperties_Get(myProp, "=TableName:");
                            string read_keyfname = MyProperties_Get(myProp, "=KeyFName:");

                            KOA_Liste_KonuOlanValue = dsKonuOlanList.Tables[read_tablename].Rows[tDataNavigator_KonuOlanList.Position][read_keyfname].ToString();
                        }
                        else
                        {
                            KOA_Liste_KonuOlanValue = "-1";
                        }
                    }

                }


                TableNo = 0;
                foreach (DataRow MSTable_Row in dsMSTables.Tables[0].Rows)
                {
                    //--------------
                    tTableCaption = MSTable_Row["TB_CAPTION"].ToString();
                    tTableCode = MSTable_Row["TABLE_CODE"].ToString();
                    tTableName = MSTable_Row["TABLE_NAME"].ToString();
                    tKey_FName = MSTable_Row["KEY_FNAME"].ToString();
                    tMaster_TableName = MSTable_Row["MASTER_TABLE_NAME"].ToString();
                    tMaster_Key_FName = MSTable_Row["MASTER_KEY_FNAME"].ToString();
                    tForeing_FName = MSTable_Row["FOREING_FNAME"].ToString();

                    tFrom = "[" + tTableName + "] [" + tTableCode + "]";
                    tWhereKeys = " 0 = 0 " + v.ENTER;
                    //if (IsNotNull(KonuOlanValue))
                    //    tWhereKeys = " [" + TableCode + "]." + tKey_FName + " = " + KonuOlanValue + " ";
                    //--------

                    if (IsNotNull(KOA_Liste_KonuOlanValue) == false)
                    {
                        RTable_Read(tForm, ref dsData, ref dsKrtr, MSTable_Row, DBName,
                                KonuOlanTableCode, KonuOlanFieldName, KonuOlanValue,
                                TableNo, tParam, UserParam);
                    }
                    else
                    {
                        RTable_Read(tForm, ref dsData, ref dsKrtr, MSTable_Row, DBName,
                                KonuOlanTableCode, KonuOlanFieldName, KOA_Liste_KonuOlanValue,
                                TableNo, tParam, UserParam);
                    }

                    TableNo++;
                }

            }

            dsMSTables.Dispose();
        }

        private string Preparing_in_select(DataSet dsMSTables, string MasterTablename)
        {
            string tTableCode = string.Empty;
            string tTableName = string.Empty;
            string tKey_FName = string.Empty;
            string tForeing_FName = string.Empty;
            string in_select = string.Empty;

            int i2 = dsMSTables.Tables[0].Rows.Count;
            for (int i = 0; i < i2; i++)
            {
                tTableName = dsMSTables.Tables[0].Rows[i]["TABLE_NAME"].ToString();

                if (tTableName == MasterTablename)
                {
                    tTableCode = dsMSTables.Tables[0].Rows[i]["TABLE_CODE"].ToString();
                    tKey_FName = dsMSTables.Tables[0].Rows[i]["KEY_FNAME"].ToString();
                    tForeing_FName = dsMSTables.Tables[0].Rows[i]["FOREING_FNAME"].ToString();

                    in_select = " select [" + tTableCode + "]." + tKey_FName + " from " + tTableName + " [" + tTableCode + "] where [" + tTableCode + "]." + tForeing_FName + " = ";
                    break;
                }
            }

            return in_select;
        }

        public string Read_REPORT_TEMP(int ID)
        {
            DataSet ds = new DataSet();

            string Sql =
               " Select REPORT_TEMP from MS_REPORTS where REF_ID = " + ID.ToString();

            SQL_Read_Execute(v.dBaseNo.Manager, ds, ref Sql, "", "Read_REPORT_TEMP");

            string temp = string.Empty;

            if (IsNotNull(ds))
            {
                temp = ds.Tables[0].Rows[0]["REPORT_TEMP"].ToString();
            }

            ds.Dispose();

            return temp;
        }

        public bool Save_REPORT_TEMP(int ID, string temp)
        {
            tSave sv = new tSave();
            DataSet dsData = new DataSet();

            // REPORT_TEMP save
            string TableName = "MS_REPORTS";
            string Sql =
               " Select * from MS_REPORTS where REF_ID = " + ID.ToString();

            // önce update olacak veriyi oku
            SQL_Read_Execute(v.dBaseNo.Manager, dsData, ref Sql, TableName, "Save_REPORT_TEMP");
            // tabloya ait field listesini oku

            // bu iptal oldu 
            //Load_Field_List(v.active_DB.managerDBName, TableName, dsData, "", "T15_MSRPR.T15_MSRPR_04");


            // sorun yok ise
            if (IsNotNull(dsData))
            {
                // atamayı yap
                dsData.Tables[0].Rows[0]["REPORT_TEMP"] = temp;

                MessageBox.Show("Burasıda MySQL den dolayı düzenleme istiyor  : Save_REPORT_TEMP() ");
            }

            dsData.Dispose();

            return true;
        }

        public void RTable_Read(Form tForm, ref DataSet dsData, ref DataSet dsKrtr, DataRow MSTable_Row,
                                string DBName,
                                string KonuOlanTableCode,
                                string KonuOlanFieldName,
                                string KonuOlanValue,
                                int TableNo,
                                Boolean tParam, string UserParam)
        {
            tSQLs sql = new tSQLs();

            DataSet dsMSFields = new DataSet();

            string function_name = "RTable Read";
            string tTableCaption = string.Empty;
            string tTableCode = string.Empty;
            string tTableName = string.Empty;
            string tKey_FName = string.Empty;
            string tMaster_TableName = string.Empty;
            string tMaster_Key_FName = string.Empty;
            string tForeing_FName = string.Empty;

            string fieldList = string.Empty;
            string foreing_where = string.Empty;
            string where_key_fname = string.Empty;

            string sqlKrtr_Kist = string.Empty;
            string sqlKrtr_Full = string.Empty;
            string tTOP_Count = string.Empty;

            tTableCaption = MSTable_Row["TB_CAPTION"].ToString();
            tTableCode = MSTable_Row["TABLE_CODE"].ToString();
            tTableName = MSTable_Row["TABLE_NAME"].ToString();
            tKey_FName = MSTable_Row["KEY_FNAME"].ToString();
            tMaster_TableName = MSTable_Row["MASTER_TABLE_NAME"].ToString();
            tMaster_Key_FName = MSTable_Row["MASTER_KEY_FNAME"].ToString();
            tForeing_FName = MSTable_Row["FOREING_FNAME"].ToString();
            foreing_where = "";// MSTable_Row["FOREING_WHERE"].ToString();  tabloya eklemeyi UNUTMA

            if (TableNo == 0) where_key_fname = tKey_FName;

            if ((TableNo > 0) && (IsNotNull(tForeing_FName)))
                where_key_fname = tForeing_FName;

            #region Read Param/Krtr

            if (tParam)
            {
                // tTableType = 1 Table ise
                sql.SQL_MSFieldsKrtr(tTableCode, ref sqlKrtr_Kist, ref sqlKrtr_Full);

                try
                {
                    Data_Read_Execute(tForm, dsKrtr, ref sqlKrtr_Kist, tTableName + "_KIST", null);
                    Data_Read_Execute(tForm, dsKrtr, ref sqlKrtr_Full, tTableName + "_FULL", null);
                    dsKrtr.Tables[tTableName + "_KIST"].Namespace = tTableCaption;
                    dsKrtr.Tables[tTableName + "_FULL"].Namespace = tTableCaption;

                }
                catch (Exception)
                {
                    //
                    throw;
                }
            }

            #endregion Read Krtr

            #region Read Data

            if (tParam == false)
            {
                /// dikkat mysql yüzünden bu yapı çalışmaz 
                /// MYSQL UNUTMA
                /// 
                fieldList = sql.SQL_MSTable_FieldsList(DBName, tTableName);

                try
                {
                    Data_Read_Execute(tForm, dsMSFields, ref fieldList, tTableName, null);
                }
                catch (Exception)
                {
                    //
                    throw;
                }

                #region Fields and ReadData
                if (IsNotNull(dsMSFields))
                {
                    string Report_FieldsName = string.Empty;
                    string fieldname = string.Empty;
                    string fcaption = string.Empty;
                    string new_Param = string.Empty;
                    string fforeing = string.Empty;
                    string tl_types_name = string.Empty;
                    string tl_value_type = string.Empty;
                    string tl_where_fname1 = string.Empty;
                    string tl_where_fname2 = string.Empty;
                    string tl_table_name = string.Empty;
                    string tl_caption_fname = string.Empty;

                    string sn = string.Empty;
                    string leftjoin = string.Empty;

                    int i = 1;

                    //d.LIST_TYPES_NAME, tl.CAPTION_FNAME, tl.VALUE_TYPE, tl.WHERE_FNAME1, tl.WHERE_FNAME2, tl.TABLE_NAME 

                    foreach (DataRow Row in dsMSFields.Tables[0].Rows)
                    {
                        sn = i.ToString();
                        fieldname = Row["name"].ToString();
                        fcaption = Set(Row["FCAPTION"].ToString(), fieldname, fieldname);
                        tl_types_name = Set(Row["LIST_TYPES_NAME"].ToString(), "", "");
                        fforeing = Row["FFOREING"].ToString();

                        if (fforeing == "True")
                            Report_FieldsName = Report_FieldsName + " , [" + tTableCode + "]." + fieldname + "  [" + fieldname + "] " + v.ENTER;

                        if (Report_FieldsName.Length == 0)
                        {
                            Report_FieldsName = Report_FieldsName + "   [" + tTableCode + "]." + fieldname + "  [" + fcaption + "] " + v.ENTER;
                        }
                        else
                        {
                            if (IsNotNull(tl_types_name) == false)
                            {
                                Report_FieldsName = Report_FieldsName + " , [" + tTableCode + "]." + fieldname + "  [" + fcaption + "] " + v.ENTER;
                            }
                            else
                            {

                                tl_caption_fname = Set(Row["CAPTION_FNAME"].ToString(), "", "");
                                tl_where_fname1 = Set(Row["WHERE_FNAME1"].ToString(), "", "");
                                tl_where_fname2 = Set(Row["WHERE_FNAME2"].ToString(), "", "");
                                tl_table_name = Set(Row["TABLE_NAME"].ToString(), "", "");

                                if ((IsNotNull(tl_caption_fname)) &&
                                    (IsNotNull(tl_where_fname1)) &&
                                    (IsNotNull(tl_table_name)))
                                {

                                    Report_FieldsName = Report_FieldsName + " , [tL" + sn + "]." + tl_caption_fname + " [" + fcaption + "] " + v.ENTER;

                                    leftjoin = leftjoin + " left outer join dbo.[" + tl_table_name + "] [tL" + sn + "] on ( [" + tTableCode + "]." + fieldname + " = "
                                      + "[tL" + sn + "]." + tl_where_fname1 + " ";

                                    if (IsNotNull(tl_where_fname2))
                                        leftjoin = leftjoin +
                                            " and " + " [tL" + sn + "]." + tl_where_fname2 + " = '" + tl_types_name + "'";

                                    leftjoin = leftjoin + " ) " + v.ENTER;
                                }
                                else
                                {
                                    Report_FieldsName = Report_FieldsName + " , [" + tTableCode + "]." + fieldname + "  [" + fcaption + "] " + v.ENTER;
                                }

                            }
                        }
                        i++;
                    }

                    tTOP_Count = "";
                    //if ((IsNotNull(UserParam) == false) && (IsNotNull(KonuOlanValue) == false))
                    if ((UserParam.IndexOf("_PARAMS:") < 0) &&
                         (KonuOlanValue == "")
                       )
                        tTOP_Count = " TOP 20 ";

                    new_Param = Preparing_UserParam(UserParam, tTableCode, KonuOlanTableCode, KonuOlanFieldName, KonuOlanValue);


                    string table_sql =
                          " Select " + tTOP_Count + v.ENTER
                        + Report_FieldsName
                        + " from [" + tTableName + "] [" + tTableCode + "]" + v.ENTER
                        + leftjoin
                        + " where 0 = 0 " + v.ENTER
                        + " /*KRITERLER*/ " + v.ENTER
                        + new_Param;

                    try
                    {
                        Data_Read_Execute(tForm, dsData, ref table_sql, tTableName, null);

                        if ((TableNo > 0) &&
                            (IsNotNull(tMaster_TableName)) &&
                            (IsNotNull(tMaster_Key_FName)) &&
                            (IsNotNull(tForeing_FName)))
                        {
                            // Master-Detail Bağlantısı
                            // Set up a master-detail relationship between the DataTables
                            DataColumn keyColumn = dsData.Tables[tMaster_TableName].Columns[tMaster_Key_FName];
                            DataColumn foreignKeyColumn = dsData.Tables[tTableName].Columns[tForeing_FName];

                            if ((keyColumn != null) && (foreignKeyColumn != null))
                            {
                                dsData.Relations.Add(tMaster_TableName + "_" + tTableName, keyColumn, foreignKeyColumn, false);
                            }
                            else
                            {
                                string s =
                                " DİKKAT : Raporlar için tablolar arası Master-Detail bağlantısı kurulamıyor. " + v.ENTER +
                                " Master Table : " + tMaster_TableName + v.ENTER +
                                " Master keyFieldName : " + tMaster_Key_FName + v.ENTER +
                                " Detail Table : " + tTableName + v.ENTER +
                                " Foreing FieldName : " + tForeing_FName + v.ENTER2;

                                if ((keyColumn == null) && (foreignKeyColumn != null))
                                    s = s + " Master tablosunun keyFieldName tespit edilemiyor..." + v.ENTER;
                                if ((keyColumn != null) && (foreignKeyColumn == null))
                                    s = s + " Detail tablosunun foringkeyFieldName tespit edilemiyor..." + v.ENTER;
                                if ((keyColumn == null) && (foreignKeyColumn == null))
                                    s = s + " Master tablosunun keyFieldName ve Detail tablosunun foringkeyFieldName tespit edilemiyor..." + v.ENTER2;

                                s = s +
                                    " 1. Database üzerinde aşağıdaki tanımla olmayabilir." + v.ENTER +
                                    "   CONSTRAINT FK_detailTableName_masterTableName FOREIGN KEY (foringkeyFieldName)  REFERENCES masterTableName (keyFieldName)" + v.ENTER +
                                    " 2. MS_TABLES tablosunda detailTableName fieldleri üzerinde -Foreing Field- tanımlaması yapılmamış olabilir..." + v.ENTER2 +
                                    " Eğer bunlar tamam ve yinede çalışmıyor ise yardım isteyin...";

                                MessageBox.Show(s, function_name);

                            }
                        }
                    }
                    catch (Exception)
                    {
                        //
                        throw;
                    }

                    dsMSFields.Dispose();

                } // (IsNotNull(dsMSFields)) 
                #endregion Fields and ReadData

            } // IsNotNull(dsData)

            #endregion Read Data

            #region Constraint örnekleri
            /* Constraint örnekleri


             DataTable customersTable = dsData.Tables["KURSIYER"];
             DataTable ordersTable = dsData.Tables["ADRES"];

             // Create unique and foreign key constraints.
             UniqueConstraint uniqueConstraintX = new
                 UniqueConstraint(customersTable.Columns["ID"]);
                        
             ForeignKeyConstraint fkConstraint = new
                     ForeignKeyConstraint("FK_ADRES_KURSIYER",
                         customersTable.Columns["ID"],
                             ordersTable.Columns["CARI_ID"]);

             fkConstraint.DeleteRule = Rule.None;
                         
             // Add the constraints.  EN SON ÇALIŞAN BU ADD OLDU
             dsData.Tables["ADRES"].Constraints.Add(fkConstraint);
                        
             // BİR TÜRLÜ ÇALIŞMADI
             //ordersTable.Constraints.AddRange(new Constraint[] { uniqueConstraintX, fkConstraint });
                        
             * 
             * --------------------------------------------------------------
             * 
             * * 
               ForeignKeyConstraint custOrderFK = new ForeignKeyConstraint("CustOrderFK",
               custDS.Tables["CustTable"].Columns["CustomerID"], 
               custDS.Tables["OrdersTable"].Columns["CustomerID"]);
               custOrderFK.DeleteRule = Rule.None;  
               // Cannot delete a customer value that has associated existing orders.
               custDS.Tables["OrdersTable"].Constraints.Add(custOrderFK);
            
            try
            {
                // Reference the tables from the DataSet.
                DataTable customersTable = dataSet.Tables["Customers"];
                DataTable ordersTable = dataSet.Tables["Orders"];

                // Create unique and foreign key constraints.
                UniqueConstraint uniqueConstraint = new 
                    UniqueConstraint(customersTable.Columns["CustomerID"]);
             
                ForeignKeyConstraint fkConstraint = new 
                    ForeignKeyConstraint("CustOrdersConstraint",
                    customersTable.Columns["CustomerID"],
                    ordersTable.Columns["CustomerID"]);

                // Add the constraints.
                customersTable.Constraints.AddRange(new Constraint[] 
                    {uniqueConstraint, fkConstraint});
            }
            catch(Exception ex)
            {
                // Process exception and return.
                Console.WriteLine("Exception of type {0} occurred.", 
                    ex.GetType());
            }

            //-------------------------------
            this.dataTable1.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "Column1"}, false)});
            

            this.dataTable2.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.ForeignKeyConstraint("Constraint1", "Table1", new string[] {
                        "Column1"}, new string[] {
                        "Column5"}, System.Data.AcceptRejectRule.None, System.Data.Rule.Cascade, System.Data.Rule.Cascade)});
            this.dataTable2.TableName = "Table2";
            
            */
            #endregion Constraint örnekleri
        }

        private string Preparing_UserParam(string UserParam, string tTableCode,
                          string KonuOlanTableCode, string KonuOlanFName, string KonuOlanValue)
        {
            string tParam1 = string.Empty;
            string tParam2 = string.Empty;

            string basamak = MyProperties_Get(UserParam, tTableCode + "_basamak:");
            string and_in = MyProperties_Get(UserParam, tTableCode + "_and_in:");
            string and_select = MyProperties_Get(UserParam, tTableCode + "_and_select:");
            string in__select = MyProperties_Get(UserParam, tTableCode + "_in__select:");

            string block = string.Empty;
            string and_block = string.Empty;
            string UserParam2 = UserParam;

            string s1 = string.Empty;
            string s2 = string.Empty;
            int i1 = 0;
            int i2 = 0;

            tTableCode = "[" + tTableCode + "]";

            if (KonuOlanTableCode.IndexOf('[') < 0)
                KonuOlanTableCode = "[" + KonuOlanTableCode + "]";

            if (((IsNotNull(KonuOlanValue)) && (tTableCode == KonuOlanTableCode)))
            {
                // tParam1 =  and [KRSYR].ID = 100  
                tParam1 = " and " + KonuOlanTableCode + "." + KonuOlanFName + " = " + KonuOlanValue + " " + v.ENTER;
            }

            if (((IsNotNull(KonuOlanValue)) && (tTableCode != KonuOlanTableCode)))
            {
                // and_in  =  and [ADRS].CARI_ID in (                 
                //     s1  =  in ( 
                //     s2  =  and [ADRS].CARI_ID 
                // tParam2 =  and [ADRS].CARI_ID = xxx 

                if ((basamak == "1") || (basamak == "2"))
                {
                    s1 = "in ( ";
                    s2 = and_in.Substring(0, and_in.Length - s1.Length);
                    tParam2 = s2 + " = " + KonuOlanValue + " " + v.ENTER;
                }

                if (basamak == "3")
                {
                    s1 = "in ( ";
                    s2 = and_in.Substring(0, and_in.Length - s1.Length);
                    tParam2 = and_in + in__select + " " + KonuOlanValue + " ) " + v.ENTER;
                    //ADRS_and_in    :   and [ADRS].CARI_ID in (
                    //ADRS_in__select:   select [KRSYR].ID from KURSIYER [KRSYR] where [KRSYR].GRUP_ID = ;   
                }

                s1 = string.Empty;
                s2 = string.Empty;
            }

            while (UserParam2.IndexOf("=ROWE_") > -1)
            {
                block = Get_And_Clear(ref UserParam2, "=ROWE_");
                and_block = MyProperties_Get(block, "_PARAMS:");

                if (and_block.Length > 0)
                {
                    if (and_block.IndexOf(tTableCode) > 0)
                    {
                        // aşağıdaki format hazırlanıyor
                        // and [GRP].TEORIK_BAS_TARIHI >=   CONVERT(SMALLDATETIME, '01.01.2015 00:00:00', 101)  
                        // and [GRP].TEORIK_BAS_TARIHI <=   CONVERT(SMALLDATETIME, '06.03.2015 00:00:00', 101) 
                        tParam1 = tParam1 + and_block;
                    }
                    else
                    {
                        // aşağıdaki formatlar hazırlanıyor
                        //=ADRS_basamak:3;
                        //=ADRS_and_in: and [ADRS].CARI_ID in ( ;
                        //=ADRS_and_select: select [ADRS].CARI_ID from ADRES [ADRS] where 0 = 0 ;
                        //=ADRS_in__select: select [KRSYR].ID from KURSIYER [KRSYR] where [KRSYR].GRUP_ID = ;


                        // ----
                        // and [GRP].ID in (
                        //    select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0  )
                        // ---
                        i1 = and_block.IndexOf("[");
                        i2 = and_block.IndexOf("]");
                        s1 = and_block.Substring(i1 + 1, (i2 - i1) - 1);
                        s2 = s1 + "_and_select:";
                        s1 = MyProperties_Get(UserParam, s2);

                        tParam2 = tParam2 +
                            and_in + v.ENTER +            //    and [GRP].ID in (
                            s1 + v.ENTER +                //    select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0 
                            and_block + " ) " + v.ENTER;  //    and [KRSYR].TAMADI like  '%tekin%'  ) 
                    }
                }
            }

            #region Açıklama
            /* gelen UserParam
             
            =BEGIN:
            =GRP_basamak:1;
            =GRP_and_in: and [GRP].ID in ( ;
            =GRP_and_select: select [GRP].ID from GRUP [GRP] where 0 = 0 ;
            =GRP_in__select:null;
            =KRSYR_basamak:2;
            =KRSYR_and_in: and [KRSYR].GRUP_ID in ( ;
            =KRSYR_and_select: select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0 ;
            =KRSYR_in__select: select [GRP].ID from GRUP [GRP] where [GRP]. = ;
            =ADRS_basamak:3;
            =ADRS_and_in: and [ADRS].CARI_ID in ( ;
            =ADRS_and_select: select [ADRS].CARI_ID from ADRES [ADRS] where 0 = 0 ;
            =ADRS_in__select: select [KRSYR].ID from KURSIYER [KRSYR] where [KRSYR].GRUP_ID = ;
            =KMLK_basamak:3;
            =KMLK_and_in: and [KMLK].CARI_ID in ( ;
            =KMLK_and_select: select [KMLK].CARI_ID from KIMLIK [KMLK] where 0 = 0 ;
            =KMLK_in__select: select [KRSYR].ID from KURSIYER [KRSYR] where [KRSYR].GRUP_ID = ;
            =KRSEK_basamak:3;
            =KRSEK_and_in: and [KRSEK].KURSIYER_ID in ( ;
            =KRSEK_and_select: select [KRSEK].KURSIYER_ID from KURSIYER_EK [KRSEK] where 0 = 0 ;
            =KRSEK_in__select: select [KRSYR].ID from KURSIYER [KRSYR] where [KRSYR].GRUP_ID = ;
            =END:

              
            =ROW_[GRP]:null;
            =[GRP]_PARAMS: and [GRP].TEORIK_BAS_TARIHI >=   CONVERT(SMALLDATETIME, '01.01.2015 00:00:00', 101)  
             and [GRP].TEORIK_BAS_TARIHI <=   CONVERT(SMALLDATETIME, '06.03.2015 00:00:00', 101)  
            ;
            =ROWE_[GRP]:null;
            =ROW_[KRSYR]:null;
            =[KRSYR]_PARAMS: and [KRSYR].TAMADI like  '%tekin%' 
             and [KRSYR].CEP_TEL like  '%532%' 
            ;
            =ROWE_[KRSYR]:null;
  
            */

            // gelen tTableCode ise ( ya  GRP  yada  KRSYR ) şeklindeki alias

            //---------------------------------------------------------

            // ortaya çıkması gereken sonuclar

            // 1. GRP için
            // main table için kriterler

            // and [GRP].TEORIK_BAS_TARIHI >=   CONVERT(SMALLDATETIME, '01.01.2015 00:00:00', 101)  
            // and [GRP].TEORIK_BAS_TARIHI <=   CONVERT(SMALLDATETIME, '06.03.2015 00:00:00', 101)  

            // and [GRP].ID in ( 
            //    select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0
            //    and [KRSYR].AD like  '%tekin%' 
            //    )

            // 2. KRSYR için

            // detail için kriterler 

            // and [KRSYR].AD like  '%tekin%' 

            // and [KRSYR].GRUP_ID in ( 
            //    select [GRP].ID from GRUP [GRP] where 0 = 0   
            //    and [GRP].TEORIK_BAS_TARIHI >=   CONVERT(SMALLDATETIME, '01.01.2015 00:00:00', 101)  
            //    and [GRP].TEORIK_BAS_TARIHI <=   CONVERT(SMALLDATETIME, '06.03.2015 00:00:00', 101)  
            //    )

            // gelen param hazırlanırken
            // master ise
            // grp      // and [GRP].ID in ( 
            // grp      //    select [GRP].ID from GRUP [GRP] where 0 = 0   
            // detail ise
            // krsyr >> // and [KRSYR].GRUP_ID in ( 
            // krsyr    //    select [KRSYR].GRUP_ID from KURSIYER [KRSYR] where 0 = 0
            #endregion Açıklama

            return (tParam1 + v.ENTER + tParam2);
        }

        public string RSQL_ParamFields(DataSet dsKrtr, int tTableType)
        {
            string s = string.Empty;

            string tTableName = string.Empty;
            string FindTable = "_FULL";
            string sFields = string.Empty;
            string sTableName = string.Empty;
            string sWhere = string.Empty;
            string idFieldName = string.Empty;

            int i2 = dsKrtr.Tables.Count;
            int i3 = 0;

            #region Table ise
            if (tTableType == 1)
            {

                for (int i = 0; i < i2; i++)
                {
                    tTableName = dsKrtr.Tables[i].TableName.ToString();

                    i3 = tTableName.IndexOf(FindTable);

                    if (i3 > 0)
                    {
                        tTableName = tTableName.Substring(0, i3);

                        // ilk fieldi tablonun id kabul ediyoruz
                        idFieldName = Set(dsKrtr.Tables[i].Rows[0]["FIELD_NAME"].ToString(), "-1", "-1");

                        if (sTableName == "")
                        {
                            sFields = " tbl" + i.ToString() + ".* ";
                            sTableName = tTableName + " tbl" + i.ToString();
                            sWhere = " tbl" + i.ToString() + "." + idFieldName + " = -1 ";
                        }
                        else
                        {
                            sFields = sFields + " , tbl" + i.ToString() + ".* ";
                            sTableName = sTableName + " , " + tTableName + " tbl" + i.ToString();
                            sWhere = sWhere + v.ENTER + " and tbl" + i.ToString() + "." + idFieldName + " = -1 ";
                        }
                    }
                }

                if (sFields.IndexOf("LKP_") == -1)
                {
                    s = " Select " + sFields + v.ENTER
                      + " from " + sTableName + v.ENTER
                      + " where " + sWhere + v.ENTER;
                }
                /* şeklinde sql hazırlanıyor
                @" select t1.* , t2.* , t3.*
                from KURSIYER t1, ADRES t2, KIMLIK t3
                where t1.ID = -1  
                and t2.ID = -1  
                and t3.ID = -1 ";
                */
            }
            #endregion Table ise

            #region Select/TableIPCode ise
            if (tTableType == 6)
            {
                for (int i = 0; i < i2; i++)
                {
                    tTableName = dsKrtr.Tables[i].TableName.ToString();
                    if (tTableName.IndexOf("_FULL") > 0)
                    {
                        string declare = string.Empty;
                        string fname = string.Empty;
                        string field_type = string.Empty;
                        string field_length = string.Empty;

                        int i5 = dsKrtr.Tables[i].Rows.Count;

                        for (int i4 = 0; i4 < i5; i4++)
                        {
                            fname = Set(dsKrtr.Tables[i].Rows[i4]["LKP_FIELD_NAME"].ToString(), "", "");
                            field_type = Set(dsKrtr.Tables[i].Rows[i4]["LKP_FIELD_TYPE"].ToString(), "0", "0");
                            field_length = Set(dsKrtr.Tables[i].Rows[i4]["LKP_FIELD_LENGTH"].ToString(), "0", "0");

                            if (field_type == "0")
                            {
                                MessageBox.Show("DİKKAT : " + fname + "  için Field Type tanımlı değil...");
                                field_type = "167";
                            }

                            declare = declare +
                                "  declare @" + fname + " " + Get_FieldTypeName(Convert.ToInt16(field_type), field_length) + v.ENTER;
                            sFields = sFields +
                                "  , " + "@" + fname + " " + fname + v.ENTER;

                        }

                        if (sFields.Length > 4)
                        {
                            // en baştaki virgülü temizle
                            sFields = sFields.Remove(0, 4);
                            sFields = "    " + sFields;
                        }

                        s = declare + v.ENTER +
                            " Select " + v.ENTER + sFields + v.ENTER;

                        break;
                    }
                }
            }

            #endregion Select/TableIPCode ise

            return s;
        }

        public string Read_ParamsValue(Form tForm, VGridControl tVGrid, string TableName)
        {

            DataSet dsKrtr = Find_DataSet(tForm, "", TableName, "");
            DataSet dsParamValue = Find_DataSet(tForm, "", "tPARAMS_" + TableName, "");

            #region Tanımlar

            string fullname = string.Empty;
            string fname = string.Empty;
            string fvalue1 = string.Empty;
            string fvalue2 = string.Empty;
            string tmpkriter = string.Empty;
            string fkriter = string.Empty;
            string foperand1 = string.Empty;
            string foperand2 = string.Empty;

            string ttable_alias = string.Empty;
            string utable_alias = string.Empty;
            string tkrt_alias = string.Empty;
            string s = string.Empty;
            string myProp = string.Empty;
            string editorType = string.Empty;
            string softCode = "";
            string projectCode = "";

            bool DeclareField = false;
            bool FirstDeclareField = false;

            int ffieldtype = 0;

            #endregion Tanımlar

            /*
              
            =ROW_[GRP]:null;
            =[GRP]_PARAMS: and [GRP].TEORIK_BAS_TARIHI >=   CONVERT(SMALLDATETIME, '01.01.2015 00:00:00', 101)  
             and [GRP].TEORIK_BAS_TARIHI <=   CONVERT(SMALLDATETIME, '06.03.2015 00:00:00', 101)  
            ;
            =ROWE_[GRP]:null;
            =ROW_[KRSYR]:null;
            =[KRSYR]_PARAMS: and [KRSYR].TAMADI like  '%tekin%' 
             and [KRSYR].CEP_TEL like  '%532%' 
            ;
            =ROWE_[KRSYR]:null;
  
            */
            int t1 = tVGrid.Rows.Count;
            int t2 = 0;

            for (int i1 = 0; i1 < t1; i1++)
            {
                t2 = tVGrid.Rows[i1].ChildRows.Count;

                #region categori altı
                for (int i2 = 0; i2 < t2; i2++)
                {
                    fvalue1 = string.Empty;
                    fvalue2 = string.Empty;
                    ffieldtype = 0;
                    foperand1 = string.Empty;
                    foperand2 = string.Empty;

                    fullname = tVGrid.Rows[i1].ChildRows[i2].Name.ToString();
                    fname = tVGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();
                    // new CheckedComboBoxEdit
                    editorType = tVGrid.Rows[i1].ChildRows[i2].Properties.RowEdit.EditorTypeName.ToString();

                    TableIPCode_Get(fullname, ref softCode, ref projectCode, ref ttable_alias, ref s);

                    if (utable_alias != ttable_alias)
                    {
                        /*
                        if (utable_alias.Length > 0)
                            fkriter = fkriter + utable_alias + "_END:" + v.ENTER;
                        
                        fkriter = fkriter + ttable_alias + "_START:" + v.ENTER;
                        */

                        if (utable_alias.Length > 0)
                        {
                            MyProperties_Set(ref myProp, "ROW_" + utable_alias, "");
                            MyProperties_Set(ref myProp, "ALIAS", utable_alias);
                            MyProperties_Set(ref myProp, utable_alias + "_PARAMS", fkriter);
                            MyProperties_Set(ref myProp, "ROWE_" + utable_alias, "");

                            fkriter = string.Empty;
                        }

                        utable_alias = ttable_alias;
                    }

                    if (tVGrid.Rows[i1].ChildRows[i2].Tag != null)
                        ffieldtype = Convert.ToInt32(tVGrid.Rows[i1].ChildRows[i2].Tag);

                    if (dsParamValue.Tables[0].Rows.Count == 2)
                    {
                        fvalue1 = dsParamValue.Tables[0].Rows[0][fname].ToString(); // başlangıç kolonu
                        fvalue2 = dsParamValue.Tables[0].Rows[1][fname].ToString(); // bitiş kolonu 
                    }

                    #region 
                    /*
                    1, '', '>='
                    2, '', '>'
                    3, '', '='
                    4, '', '<='
                    5, '', '<'
                    6, '', 'Benzerleri (%abc%)'
                    7, '', 'Benzerleri (abc%)'
                    8, '', '<>'
                    */
                    #endregion

                    if (IsNotNull(fvalue1) && IsNotNull(fvalue2))
                    {
                        //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239
                        if ((ffieldtype == 175) | (ffieldtype == 167) | (ffieldtype == 99) | (ffieldtype == 35) | (ffieldtype == 239))
                        {
                            foperand1 = " like ";
                            foperand2 = " like ";
                        }
                        else
                        {
                            foperand1 = " >= ";
                            foperand2 = " <= ";
                        }
                    }

                    if (IsNotNull(fvalue1) && IsNotNull(fvalue2) == false)
                    {

                        //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239
                        if ((ffieldtype == 175) | (ffieldtype == 167) | (ffieldtype == 99) | (ffieldtype == 35) | (ffieldtype == 239))
                            foperand1 = " like ";
                        else foperand1 = " = ";

                    }

                    // new CheckedComboBoxEdit
                    if (editorType == "CheckedComboBoxEdit")
                    {
                        foperand1 = " in ";
                    }

                    if (IsNotNull(fvalue1))
                    {
                        DeclareField = DeclarePreparingField(fullname, fvalue1, ffieldtype);
                        FirstDeclareField = DeclareField;

                        if (DeclareField == false)
                        {
                            if (foperand1 != " like ")
                            {
                                fvalue1 = tData_Convert(fvalue1, ffieldtype);
                                fullname = tField_Convert(fullname, ffieldtype);
                                fkriter = fkriter + " and " + fullname + foperand1 + fvalue1 + v.ENTER;
                            }
                            if (foperand1 == " like ")
                            {
                                fvalue1 = tData_Convert("%" + fvalue1 + "%", ffieldtype);
                                fkriter = fkriter + " and " + fullname + foperand1 + fvalue1 + v.ENTER;
                            }
                            // new CheckedComboBoxEdit
                            if (foperand1 == " in ")
                            {
                                //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239
                                if ((ffieldtype == 175) | (ffieldtype == 167) | (ffieldtype == 99) | (ffieldtype == 35) | (ffieldtype == 239))
                                    fvalue1 = tData_Convert(fvalue1, 1672);

                                //* rakam  56, 48, 127, 52, 60, 62, 59, 106, 108
                                if ((ffieldtype == 56) | (ffieldtype == 48) | (ffieldtype == 127) | (ffieldtype == 52) |
                                    (ffieldtype == 60) | (ffieldtype == 62) | (ffieldtype == 59) | (ffieldtype == 106) | (ffieldtype == 108))
                                    fvalue1 = tData_Convert(fvalue1, 562);

                                fkriter = fkriter + " and " + fullname + foperand1 + fvalue1 + v.ENTER;
                            }
                        }
                        else
                        {
                            // DeclareField yüzünden boş döndüğünde DataRead çalışmıyor
                            // çalışsın diye
                            fkriter = fkriter + "  ";
                        }
                    }

                    if (IsNotNull(fvalue2) && (FirstDeclareField == false))
                    {
                        if (foperand2 != " like ")
                        {
                            fvalue2 = tData_Convert(fvalue2, ffieldtype);
                            fullname = tField_Convert(fullname, ffieldtype);
                            fkriter = fkriter + " and " + fullname + foperand2 + fvalue2 + v.ENTER;
                        }
                        if (foperand2 == " like ")
                        {
                            fvalue2 = tData_Convert("%" + fvalue2 + "%", ffieldtype);
                            fkriter = fkriter + " and " + fullname + foperand2 + fvalue2 + v.ENTER;
                        }
                    }

                    // --------------------------

                }
                #endregion categori altı 

            }

            if (fkriter.Length > 0)
            {
                MyProperties_Set(ref myProp, "ROW_" + utable_alias, "");
                MyProperties_Set(ref myProp, "ALIAS", utable_alias);
                MyProperties_Set(ref myProp, ttable_alias + "_PARAMS", fkriter);
                MyProperties_Set(ref myProp, "ROWE_" + ttable_alias, "");

                //    fkriter = fkriter + utable_alias + "_END:" + v.ENTER;
            }

            return myProp; // fkriter;
        }

        public void Clear_ParamsValue(Form tForm, string TableName)
        {
            DataSet dsParamValue = Find_DataSet(tForm, "", "tPARAMS_" + TableName, "");
            if (IsNotNull(dsParamValue))
            {
                dsParamValue.Tables[0].Rows.Clear();
                dsParamValue.Tables[0].Rows.Add();
                dsParamValue.Tables[0].Rows.Add();
            }
        }


        #endregion Report.Table işlemleri

        #region AllFormsClose, WaitForm

        public void AllFormsClose()
        {
            int i2 = Application.OpenForms.Count;

            for (int i = i2 - 1; i > 0; i--)
            {
                string formType = Application.OpenForms[i].GetType().ToString();
                //DevExpress.XtraWaitForm.AutoLayoutDemoWaitForm
                if (formType.IndexOf("XtraWaitForm") == -1 &&
                    formType.IndexOf("AlertForm") == -1)
                    Application.OpenForms[i]?.Dispose();
            }
        }

        public void WaitFormOpen(Form tForm, string Mesaj)
        {
            if (Cursor.Current != Cursors.Default)
                Cursor.Current = Cursors.Default;

            if (Mesaj == "") Mesaj = "İşlemler devam ediyor...";

            if (v.IsWaitOpen)
            {
                //SplashScreenManager.Default.SetWaitFormCaption(" " + Mesaj);
                SplashScreenManager.Default?.SetWaitFormDescription(v.ENTER + "  " + Mesaj);
                return;
            }

            //Mesaj = "  " + Mesaj.PadRight(100);

            if (v.SP_OpenApplication == false)
            {
                if (tForm != null)
                {
                    SplashScreenManager.ShowForm(tForm, typeof(DevExpress.XtraWaitForm.AutoLayoutDemoWaitForm), true, true, false);
                    SplashScreenManager.Default.SetWaitFormCaption(" " + Mesaj);
                    if (v.SP_TabimDbConnection)
                        SplashScreenManager.Default.SetWaitFormDescription(v.ENTER + "  " + "Tabim.MTSK");
                    else SplashScreenManager.Default.SetWaitFormDescription(v.ENTER + "  " + "Lütfen bekleyiniz...");
                }
                //else
                //    SplashScreenManager.ShowDefaultSplashScreen();

                //SplashScreenManager.Default.SetWaitFormCaption("üstadın yeşiL defteri");
                //SplashScreenManager.Default.SetWaitFormDescription(Mesaj);

                //SplashScreenManager.ShowForm(tForm, typeof(DevExpress.XtraWaitForm.ManualLayoutDemoWaitForm), true, true, false);
                //SplashScreenManager.ShowForm(tForm, typeof(DevExpress.XtraWaitForm.DemoWaitForm), true, true, false);

                //SplashScreenManager.ShowForm(tForm, typeof(DevExpress.XtraSplashScreen.DemoProgressSplashScreen), true, true, false);
                //SplashScreenManager.ShowForm(tForm, typeof(), true, true, false);
                //SplashScreenManager.ShowDefaultProgressSplashScreen(Mesaj);
            }

                //SplashScreenManager.ShowDefaultProgressSplashScreen(Mesaj);

                //SplashScreenManager.Default.SetWaitFormCaption(v.Wait_Caption);
                //SplashScreenManager.Default.SetWaitFormDescription(Mesaj);

                // acildı.
                v.IsWaitOpen = true;
        }

        public void WaitFormClose()
        {
            if (v.IsWaitOpen) return;
            SplashScreenManager.CloseForm(false);
        }

        public void WaitFormClose(int sleepCount)
        {
            if (sleepCount == 0) sleepCount = 1000;
            Thread.Sleep(sleepCount);
            SplashScreenManager.CloseForm(false);
        }
        #endregion AllFormsClose

        #region getUserLookAndFeelSkins
        public void getUserLookAndFeelSkins()
        {
            if (v.sp_activeSkinName != "Form_Activated")
            {
                if (UserLookAndFeel.Default.ActiveSkinName.ToString().IndexOf(v.sp_deactiveSkinName) > -1) return;
                if (UserLookAndFeel.Default.ActiveSvgPaletteName.ToString().IndexOf(v.sp_deactiveSkinName) > -1) return;
            }
            else
                v.sp_activeSkinName = "";

            // Application.OpenForms[0].Text = Application.OpenForms[0].Text + ",g";

            // kayıtlı olan yükleniyor
            //
            tRegistry reg = new tRegistry();
            object skinUserFirm = null;

            //object skin = reg.getRegistryValue("userSkin");
            //object skinUser = reg.getRegistryValue("userSkin_" + v.tUser.UserId.ToString());
            //if (skin != null)
            //    UserLookAndFeel.Default.SetSkinStyle(skin.ToString());
            //if (skinUser != null)
            //    UserLookAndFeel.Default.SetSkinStyle(skinUser.ToString());

            skinUserFirm = reg.getRegistryValue("userSkin_" + v.tUser.UserId.ToString() + "_" + v.tMainFirm.FirmId.ToString());  //v.SP_FIRM_ID.ToString());

            if (skinUserFirm != null)
            {
                // okunan style yüklensin
                //
                if (skinUserFirm.ToString().IndexOf("||") > -1)
                {
                    string paletteName = skinUserFirm.ToString();
                    string skinName = Get_And_Clear(ref paletteName, "||");

                    DevExpress.LookAndFeel.UserLookAndFeel.Default.UseDefaultLookAndFeel = true;

                    UserLookAndFeel.Default.SetSkinStyle(skinName, paletteName);
                }
                else
                {
                    UserLookAndFeel.Default.SetSkinStyle(skinUserFirm.ToString());
                }

                // yükleme bittiği için kendinisini null yapalım
                skinUserFirm = null;
            }
            //this.Text = this.Text + "  " + skinUserFirm.ToString();
        }

        #endregion

        #region getUserInfo

        public void getUserInfo()
        {
            string tSql = "";
            tSQLs sql = new tSQLs();
            DataSet ds = new DataSet();
            
            if (v.SP_TabimDbConnection)
            {
                tSql = sql.preparingTabimUsersSql("", "", v.tUser.UserId); // UserId ile giriş
                SQL_Read_Execute(v.dBaseNo.Local, ds, ref tSql, "TabimUsers", "FindUser");
            }
            else
            {
                //tSql = sql.preparingUstadUsersSql("", "", v.tUser.UserId);
                //SQL_Read_Execute(v.dBaseNo.Project, ds, ref tSql, "Users", "FindUser");
            }

            if (IsNotNull(ds))
            {
                if (v.SP_TabimDbConnection)
                {
                    getTabimUserAbout(ds);
                }
                else
                {
                    //
                }
            }

        }

        public void getTabimUserAbout(DataSet ds)
        {
            if (IsNotNull(ds))
            {
                // Surucu07.users tablosunun önceki bilgileri
                v.tUser.UserId = myInt32(ds.Tables[0].Rows[0]["Ulas"].ToString());
                v.tUser.IsActive = Convert.ToBoolean(ds.Tables[0].Rows[0]["AKTIF"].ToString());
                v.tUser.Username_ = ds.Tables[0].Rows[0]["Username_"].ToString();
                // Bunlar yesildefter tarafdından eklenen bilgiler
                v.tUser.UserGUID = ds.Tables[0].Rows[0]["UserGUID"].ToString();
                v.tUser.UserFirmGUID = ds.Tables[0].Rows[0]["FirmGUID"].ToString();
                v.tUser.FullName = ds.Tables[0].Rows[0]["UserFullName"].ToString();
                v.tUser.FirstName = ds.Tables[0].Rows[0]["UserFirstName"].ToString();
                v.tUser.LastName = ds.Tables[0].Rows[0]["UserLastName"].ToString();
                v.tUser.UserTcNo = ds.Tables[0].Rows[0]["UserTcNo"].ToString();
                v.tUser.eMail = ds.Tables[0].Rows[0]["UserEMail"].ToString();
                v.tUser.MobileNo = ds.Tables[0].Rows[0]["UserMobileNo"].ToString();
                v.tUser.UserDbTypeId = myInt16(ds.Tables[0].Rows[0]["DbTypeId"].ToString());
                v.tUser.MebbisCode = ds.Tables[0].Rows[0]["MebbisCode"].ToString();
                v.tUser.MebbisPass = ds.Tables[0].Rows[0]["MebbisPass"].ToString();
                //v.tUser.Key = u_db_user_key;
                //v.tUser.UserDbTypeId = userDbTypeId;
            }
        }

        #endregion getUserInfo

        #region Expression - Column üzerindeki formüller hesaplanıyor

        private class vExpression
        {
            public Form tForm { get; set; }
            public Control cntrl { get; set; }
            public Control cntrlExt { get; set; }
            public DataSet dsData { get; set; }
            public DataSet dsDataExt { get; set; }
            public int pos { get; set; }
            public string exp_type { get; set; }
            public string exp_value { get; set; }
            public string focus_field { get; set; }
            public string exp_formul_fname { get; set; }
            public string extra_fname { get; set; }
            public string send_FieldName { get; set; }
            public string send_Value { get; set; }
            public string pa { get; set; }
            public string pk { get; set; }
            public string caption { get; set; }

        }

        public void work_EXPRESSION(Form tForm, string TableIPCode, string fieldName, string newValue)
        {
            DataSet dsData = null;
            DataNavigator tDataNavigator = null;
            Find_DataSet(tForm, ref dsData, ref tDataNavigator, TableIPCode);

            if ((IsNotNull(dsData)) &&
                (tDataNavigator != null))
            {
                //Application.OpenForms[0].Text = "Expression : " + e.Column.FieldName + " 1,";

                v.Kullaniciya_Mesaj_Var = "Hesaplar çalışıyor : " + fieldName;

                v.con_Expression_View = "1. Faz ------- " + v.ENTER;
                v.con_Expression_Send_Value = newValue;
                v.con_Expression = true;
                Preparing_Expression(tForm, dsData, tDataNavigator.Position, TableIPCode, fieldName, v.con_Expression_Send_Value);
                /// bazı formullerde kendinden sonraki fieldlerden değer alıyor 
                /// fakat henüz orada bir işlem yapılmamış olduğu için işlem eksik kalıyor
                /// ikinci defa tekrar hesaplamaya gidince aradığı değeri bulmuş oluyor
                v.con_Expression_View = v.con_Expression_View + "2. Faz ------- " + v.ENTER;
                v.con_Expression = true;
                Preparing_Expression(tForm, dsData, tDataNavigator.Position, TableIPCode, fieldName, v.con_Expression_Send_Value);
                /// -----
                /// 
            }
        }

        public void Preparing_Expression(Form tForm, DataSet dsData, int pos,
                              string TableIPCode, string send_FieldName, string send_Value)
        {
            // EXPRESSION = formül
            // send_FieldName = şu anda veri girişi yapılan field yani change olan
            // send_Value     = şu anda girilen veri 
            // (v.con_Expression == false) = şu anda aşağıda işlem yapılan fielde hesaplalnlar set ediliyorsa 

            if ((pos == -1) || (v.con_Expression == false)) return;
            
            if ((send_FieldName != "") && (send_Value == "")) 
                send_Value = "0";

            List<PROP_EXPRESSION> prop_ = null;

            string myProp = dsData.Namespace.ToString();
            string tableName = MyProperties_Get(myProp, "TableName:");
            
            int i4 = v.ds_MsTableFields.Tables[tableName].Rows.Count;

            string send_prop_Expression = prop_Expression_Get(dsData, TableIPCode, send_FieldName); // new


            // DataLayoutControl ise
            #region yapılan değişikliği view üzerine işle

            Control cntrl = null;
            cntrl = Find_Control_View(tForm, TableIPCode);

            Control cntrlExt = null;
            string External_TableIPCode = Find_External_TableIPCode(dsData);
            if (IsNotNull(External_TableIPCode))
            {
                cntrlExt = Find_Control_View(tForm, External_TableIPCode);
            }

            #endregion

            string s2 = (char)34 + "EXP_TYPE" + (char)34 + ":";
            /// "EXP_TYPE":

            /// formül iki şekilde olabilir
            /// birincisi  [

            vExpression vExp = new vExpression();

            vExp.tForm = tForm;
            vExp.cntrl = cntrl;
            vExp.cntrlExt = cntrlExt;
            vExp.dsData = dsData;
            vExp.pos = pos;
            vExp.send_FieldName = send_FieldName;
            vExp.send_Value = send_Value;

            //List<PROP_EXPRESSION> packet = new List<PROP_EXPRESSION>();

            int p = send_prop_Expression.IndexOf("[");
            if ((p == -1) || (p > 10))
                send_prop_Expression = "[" + send_prop_Expression + "]";

            //List<PROP_EXPRESSION> prop_ = t.readPropList<PROP_EXPRESSION>(send_prop_Expression);


            #region // tablonu fieldleri

            string prop_Expression = string.Empty;
            string exp_formul_fname = string.Empty;

            /// Sırayla tüm fieldleri kontrol edelim 
            /// fielde ait bir formul var çalıştırılacak
            for (int i = 0; i < i4; i++)
            {
                /// EXP_TYPE 
                /// COMP = Compute
                /// SETDATA = Set Data
                vExp.exp_type = "";
                
                /// "formül gelecek";
                vExp.exp_value = "";
                
                /// formülün sahibi olan field
                exp_formul_fname = v.ds_MsTableFields.Tables[tableName].Rows[i]["name"].ToString();

                /// formül okunuyor 
                prop_Expression = prop_Expression_Get(dsData, TableIPCode, exp_formul_fname);

                /// formül varsa
                if (IsNotNull(prop_Expression))
                {
                    vExp.exp_formul_fname = exp_formul_fname;
                    vExp.extra_fname = "";

                    //v.con_Expression_View = v.con_Expression_View + "  " + exp_formul_fname + " : field " + v.ENTER;

                    // json format varsa
                    if (prop_Expression.IndexOf(s2) > -1)
                    {
                        // prop_Expression : eğer birden fazla ( JSON ) yorum ve hesap varsa 
                        // sırayla işlem yapması için Exp_Preparinge git ve orada her json satırı için teker teker hesapla

                        prop_ = readPropList<PROP_EXPRESSION>(prop_Expression);

                        vExp.pa = "<";
                        vExp.pk = ">";
                        vExp.exp_value = "";
                        Exp_Preparing(vExp, prop_);
                    }
                    else
                    {
                        // prop_Expression içinde sadece tek bir formul var ise direk buradan git çalış

                        // json değilde düz yazılmış formul ise
                        vExp.pa = "[";
                        vExp.pk = "]";
                        vExp.exp_value = prop_Expression;
                        vExp.focus_field = "FALSE"; // tek formul olduğu için
                        Exp_Set(vExp);
                    }

                    v.con_Expression_View =
                        v.con_Expression_View + v.ENTER + "-------------- " + v.ENTER;

                } //if (t.IsNotNull(prop_Expression))
            } // for 

            #endregion
            
        }

        private string Exp_Preparing(vExpression vExp, List<PROP_EXPRESSION> prop_)
        {

            bool onay = false;
            string formul_sahibi = vExp.exp_formul_fname.ToString();
            string extra_FName = string.Empty;

            // FIRST
            string chc_IPCode = string.Empty;
            string chc_FName = string.Empty;
            string chc_Value = string.Empty;
            string chc_Operand = string.Empty;
            // SEC = SECOND
            string chc_IPCode_SEC = string.Empty;
            string chc_FName_SEC = string.Empty;
            string chc_Value_SEC = string.Empty;
            string chc_Operand_SEC = string.Empty;

            foreach (PROP_EXPRESSION item in prop_)
            {
                onay = false;
                chc_IPCode = item.CHC_IPCODE.ToString();
                chc_FName = item.CHC_FNAME.ToString();
                chc_Value = item.CHC_VALUE.ToString();
                chc_Operand = item.CHC_OPERAND.ToString();
                chc_IPCode_SEC = item.CHC_IPCODE_SEC.ToString();
                chc_FName_SEC = item.CHC_FNAME_SEC.ToString();
                chc_Value_SEC = item.CHC_VALUE_SEC.ToString();
                chc_Operand_SEC = item.CHC_OPERAND_SEC.ToString();

                onay = Exp_Check(vExp.dsData, vExp.pos
                    , chc_FName
                    , chc_Value
                    , chc_Operand
                    , chc_FName_SEC
                    , chc_Value_SEC
                    , chc_Operand_SEC);

                if (onay == true)
                {
                    /// EXP_TYPE 
                    /// COMP = Compute
                    /// SETDATA = Set Data
                    vExp.caption = item.CAPTION.ToString();
                    vExp.exp_type = item.EXP_TYPE.ToString();
                    vExp.exp_value = item.EXP_VALUE.ToString(); // bir satırdaki formul;
                    vExp.extra_fname = item.EXTRA_FNAME.ToString();
                    vExp.focus_field = item.FOCUS_FIELD.ToString();

                    if (vExp.exp_value == "null")
                    {
                        MessageBox.Show("DİKKAT : " + formul_sahibi + " field\'in ( " + item.CAPTION.ToString() + " ) ait formül satır bulunamadı... ");
                    }

                    /// burdan itibaren bu döngüdeki formülü çalıştırmaya gidiyor 
                    /// birden fazla yorum ve hesap olabilir
                    Exp_Set(vExp);
                }

            }

            return "";
        }

        private void Exp_Set(vExpression vExp)
        {
            // bir satırdaki formul;
            if (IsNotNull(vExp.exp_value) == false) return;

            int p1 = 0;
            int p2 = 0;
            string s = "";
            string fname = "";
            string workFName = vExp.exp_formul_fname.ToString();
            string value = "";

            // eğer başka bir field için işlem yapılacak ise
            if (IsNotNull(vExp.extra_fname.ToString()))
                workFName = vExp.extra_fname.ToString();

            // formül bulundu ise 
            // yorum içinde workFName yi kullanma esas olarak vExp.exp_formul_fname üzerinde işlem yapılıyor
            if (
                (vExp.exp_value != "") &&
                ((vExp.exp_formul_fname != vExp.send_FieldName) && (vExp.focus_field == "FALSE")) ||
                ((vExp.exp_formul_fname == vExp.send_FieldName) && (vExp.focus_field == "TRUE"))
               )
            {
                v.con_Expression_View =
                    v.con_Expression_View + "  " + vExp.exp_type + " : " + workFName + " := " + vExp.exp_value;

                //if (workFName == "ISKONTO_ORANI")
                //{
                //    // maksat durdurmak
                //    v.Kullaniciya_Mesaj_Var = workFName;
                //}

                #region formul üzerindeki isimlerin yerine değerleriyle (value) değiştir
                //while (exp_value.IndexOf("<") > -1)
                while (vExp.exp_value.IndexOf(vExp.pa) > -1)
                {
                    //p1 = exp_value.IndexOf("<");
                    //p2 = exp_value.IndexOf(">");
                    p1 = vExp.exp_value.IndexOf(vExp.pa);
                    p2 = vExp.exp_value.IndexOf(vExp.pk);

                    if ((p1 > -1) && (p2 > -1))
                    {
                        fname = vExp.exp_value.Substring(p1 + 1, (p2 - p1) - 1);
                        if (vExp.pos > -1)
                        {
                            value = vExp.dsData.Tables[0].Rows[vExp.pos][fname].ToString();

                            // şuan girilen değeri al
                            if (fname == vExp.send_FieldName)
                                value = vExp.send_Value;

                            if (value == "") value = "0";
                            if (value.ToUpper() == "TRUE") value = "1";
                            if (value.ToUpper() == "FALSE") value = "0";

                            //t.Str_Replace(ref exp_value, "<" + fname + ">", value);
                            s = vExp.exp_value;
                            Str_Replace(ref s, vExp.pa + fname + vExp.pk, value);

                            vExp.exp_value = s;
                        }
                    }

                    vExp.exp_value = vExp.exp_value.Replace(".AddDays", "||");
                    vExp.exp_value = vExp.exp_value.Replace("AddDays", "||");
                    vExp.exp_value = vExp.exp_value.Replace(".Subtract", "||");
                    vExp.exp_value = vExp.exp_value.Replace("Subtract", "||");
                }
                #endregion formül üzerindeki

                #region formülü hesapla ve ilgili fielde ata
                s = vExp.exp_value;
                Str_Replace(ref s, ",", ".");
                vExp.exp_value = s;

                #region COMP
                if ((vExp.exp_type == "") || (vExp.exp_type == "COMP"))
                {
                    value = JScriptEval(vExp.exp_value).ToString();
                    if ((value == "") || (value == "NaN")) value = "0";

                    v.con_Expression_View = v.con_Expression_View + " : " + vExp.caption.ToString() + " :: " + vExp.exp_value + " := " + value + v.ENTER;
                }
                #endregion

                #region SETDATA
                if (vExp.exp_type == "SETDATA")
                {
                    // exp_value while içinde temizlik için işleme başladığında gerçek değeride okunmuş oluyor
                    // <ISK_KADEME_ORANI>  >>>  t.Str_Replace(ref exp_value, "<" + fname + ">", value);  
                    // yukardaki satırda atanmak istenen değer zaten okunmuş ve atamaya hazır durumda bekliyor

                    if (vExp.exp_value != "")
                    {
                        //    value = vExp.exp_value;
                        vExp.dsData.Tables[0].Rows[vExp.pos][workFName] = System.Convert.ToDecimal(vExp.exp_value);

                        v.con_Expression_View = v.con_Expression_View + " := " + vExp.exp_value + v.ENTER;

                        if ((vExp.focus_field == "TRUE") &&
                            (workFName == vExp.send_FieldName.ToString())
                            //(vExp.exp_formul_fname.ToString() == vExp.send_FieldName.ToString())
                            )
                        {
                            vExp.send_Value = vExp.exp_value;
                            v.con_Expression_Send_Value = vExp.exp_value;
                        }
                        //MessageBox.Show(vExp.exp_formul_fname.ToString() + ";" + vExp.send_FieldName.ToString());
                        /// Burası aşağıdaki madde için yapıldı
                        ///"CAPTION": "fazla ise",
                        ///"EXP_TYPE": "SETDATA",
                        ///"EXP_VALUE": "10",
                        ///"EXTRA_FNAME": "null",
                        ///"FOCUS_FIELD": "TRUE",
                        ///"CHC_IPCODE": "null",
                        ///"CHC_FNAME": "TEVKIFAT_PAY",
                        ///"CHC_VALUE": "10",
                        ///"CHC_OPERAND": ">",
                    }

                }
                #endregion

                #region DATEADDDAYS
                // <BelgeTarihi>.AddDays<VadeGun>
                // DateTime answer = today.AddDays(10);
                if (vExp.exp_type == "DATEADDDAYS")
                {
                    //MessageBox.Show(vExp.exp_value);
                    string values = vExp.exp_value + "||";
                    string tarih = "";
                    string vadeGun = "";

                    if (values.IndexOf("||") > -1)
                    {
                        tarih = Get_And_Clear(ref values, "||");
                        vadeGun = Get_And_Clear(ref values, "||");
                        if (IsNotNull(tarih) && IsNotNull(vadeGun))
                        {
                            try
                            {
                                DateTime belgeTarihi = Convert.ToDateTime(tarih);
                                int gun = myInt32(vadeGun);
                                value = belgeTarihi.AddDays(gun).Date.ToString();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(vExp.exp_formul_fname + " : " + workFName + " = " + value + " ; " + v.ENTER2 + e.Message, "Uyarı");
                                //throw;
                            }
                        }
                    }

                }
                #endregion

                #region DATESUBTRACT
                // <BelgeTarihi>Subtract<SonOdemeTarihi>
                // var result = answer.Subtract(today).TotalDays;
                if (vExp.exp_type == "DATESUBTRACT")
                {
                    //MessageBox.Show(vExp.exp_value);
                    string values = vExp.exp_value + "||";
                    string tarih1 = "";
                    string tarih2 = "";

                    if (values.IndexOf("||") > -1)
                    {
                        tarih1 = Get_And_Clear(ref values, "||");
                        tarih2 = Get_And_Clear(ref values, "||");

                        // tarih1 = 0 olursa
                        //if ((tarih1.Length == 1) && (tarih2.Length > 1))
                        //    tarih1 = tarih2;

                        //if (t.IsNotNull(tarih1) && t.IsNotNull(tarih2))
                        if ((tarih1.Length > 10) && (tarih2.Length > 10))
                        {
                            try
                            {
                                DateTime sonTarih = Convert.ToDateTime(tarih1);
                                DateTime belgeTarihi = Convert.ToDateTime(tarih2);
                                value = sonTarih.Subtract(belgeTarihi).TotalDays.ToString();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(vExp.exp_formul_fname + " : " + workFName + " = " + value + " ; " + v.ENTER2 + e.Message, "Uyarı");
                                //throw;
                            }
                        }
                    }
                }
                #endregion

                try
                {
                    //if (workFName == "ISKONTO_ORANI")
                    //if (workFName == "ISKONTO_TUTARI")
                    //{
                    //    // maksat durdurmak
                    //v.Kullaniciya_Mesaj_Var = workFName;
                    //}

                    v.con_Expression = false;

                    if ((vExp.exp_type == "") ||
                        (vExp.exp_type == "COMP"))
                    {
                        if ((value != "") && (value != "null"))
                        {
                            vExp.dsData.Tables[0].Rows[vExp.pos][workFName] = System.Convert.ToDecimal(value);
                        }
                    }
                    
                    if ((vExp.exp_type == "SETDATA") ||
                        (vExp.exp_type == "DATEADDDAYS") ||
                        (vExp.exp_type == "DATESUBTRACT"))
                    {
                        if ((value != "") && (value != "null"))
                        {
                            vExp.dsData.Tables[0].Rows[vExp.pos][workFName] = value;
                        }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(vExp.exp_formul_fname + " : " + workFName + " = " + value + " ; " + v.ENTER2 + e.Message, "Uyarı");
                }
                //t.Str_Replace(ref value, ",", ".");
                #endregion

                // DataLayoutControl ise
                #region yapılan değişikliği view üzerine işle
                if (vExp.cntrl != null)
                {
                    if (vExp.cntrl.GetType().ToString() == "DevExpress.XtraDataLayout.DataLayoutControl")
                    {
                        DataLayoutControl_Refresh(vExp.tForm, vExp.cntrl, workFName, value);
                        if (IsNotNull(vExp.send_FieldName))
                            DataLayoutControl_Refresh(vExp.tForm, vExp.cntrl, vExp.send_FieldName, vExp.send_Value);
                    }
                }
                if (vExp.cntrlExt != null)
                {
                    if (vExp.cntrlExt.GetType().ToString() == "DevExpress.XtraDataLayout.DataLayoutControl")
                    {
                        DataLayoutControl_Refresh(vExp.tForm, vExp.cntrlExt, workFName, value);
                        if (IsNotNull(vExp.send_FieldName))
                            DataLayoutControl_Refresh(vExp.tForm, vExp.cntrlExt, vExp.send_FieldName, vExp.send_Value);
                    }
                }

                #endregion
            }
        }

        private bool Exp_Check(
              DataSet dsData, int pos
            , string chc_FName
            , string chc_Value
            , string chc_Operand
            , string chc_FName_SEC
            , string chc_Value_SEC
            , string chc_Operand_SEC
            )
        {
            bool onay1 = true;
            bool onay2 = true;

            string read_value = "";

            string displayFormat = string.Empty;
            int ftype = 0;

            #region Check 1 işlemleri 
            if (//t.IsNotNull(chc_IPCode) &&
                (IsNotNull(chc_FName)) &&
                (IsNotNull(chc_Value)))
            {
                ftype = Find_Field_Type_Id(dsData, chc_FName, ref displayFormat);

                read_value = dsData.Tables[0].Rows[pos][chc_FName].ToString();

                if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                    (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 106) | (ftype == 108) && read_value == "")
                    read_value = "0";

                if (read_value != "")
                {
                    if ((chc_Value.IndexOf(read_value) > -1) &&
                        (chc_Operand == ""))
                    {
                        // eğer buraya kadar geldiyse 
                        // öndeki chc_xxxx kontrollerinden geçti,  
                        // yani onayı hak etti demektir
                        onay1 = true;
                    }

                    if (IsNotNull(chc_Operand))
                    {
                        onay1 = myOperandControl(read_value, chc_Value, chc_Operand);
                    }
                }
            }
            #endregion Check işlemleri1

            #region Check 2 işlemleri 
            if (//t.IsNotNull(chc_IPCode_SEC) &&
                (IsNotNull(chc_FName_SEC)) &&
                (IsNotNull(chc_Value_SEC)))
            {
                ftype = Find_Field_Type_Id(dsData, chc_FName_SEC, ref displayFormat);

                read_value = dsData.Tables[0].Rows[pos][chc_FName_SEC].ToString();

                if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) |
                    (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 106) | (ftype == 108) && read_value == "")
                    read_value = "0";

                if (read_value != "")
                {
                    if ((chc_Value_SEC.IndexOf(read_value) > -1) &&
                        (chc_Operand_SEC == ""))
                    {
                        // eğer buraya kadar geldiyse 
                        // öndeki chc_xxxx kontrollerinden geçti,  
                        // yani onayı hak etti demektir
                        onay2 = true;
                    }

                    if (IsNotNull(chc_Operand_SEC))
                    {
                        onay2 = myOperandControl(read_value, chc_Value_SEC, chc_Operand_SEC);
                    }
                }
            }
            #endregion Check işlemleri2

            return (onay1 && onay2);
        }

        private double JScriptEval(string expr)
        {
            Microsoft.JScript.Vsa.VsaEngine myEngine = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();

            double sonuc = 0;
            try
            {
                sonuc = double.Parse(Microsoft.JScript.Eval.JScriptEvaluate(expr, myEngine).ToString());
            }
            catch (Exception)
            {
                sonuc = 0;
                //throw;
            }

            return sonuc;

            // error checking etc removed for brevity
            //return double.Parse(Eval.JScriptEvaluate(expr, _engine).ToString());
            //private readonly VsaEngine _engine = VsaEngine.CreateEngine();
        }

        private string prop_Expression_Get(DataSet ds, string tableIPCode, string fName)
        {
            string s = "";
            int j = ds.Tables[tableIPCode].Rows.Count;
            for (int i = 0; i < j; i++)
            {
                if (ds.Tables[tableIPCode].Rows[i]["LKP_FIELD_NAME"].ToString() == fName)
                {
                    s = ds.Tables[tableIPCode].Rows[i]["LKP_PROP_EXPRESSION"].ToString();
                    break;
                }
            }
            
            return s;
        }

        #endregion EXPRESSION

        #region DataLayoutControl_Refresh
        public void DataLayoutControl_Refresh(Form tForm, Control cntrl, string FieldName, string Value)
        {
            //Application.OpenForms[0].Text += FieldName + ";";
            foreach (Control item in ((DevExpress.XtraDataLayout.DataLayoutControl)cntrl).Controls)
            {
                System.Windows.Forms.Binding b = item.DataBindings["EditValue"];
                if (b != null)
                {
                    v.con_Expression = false;
                    if (item.Name == "Column_" + FieldName)
                        item.Text = Value;
                    v.con_Expression = false;
                    b.WriteValue();
                }
            }
        }
        #endregion

        #region WebBrowser

        /// <summary>
        /// WebSet kullanımı
        /// </summary>
        /// tSetAttribute ws = new tSetAttribute();
        /// ---
        /// ws._01_Caption = "Mevcut Ehliyet Sınıfı";
        /// ws._02_ElemanName = "cmbMevcutEhliyetSinifi";
        /// ws._03_SetValue = MevcutSertifikaValue;
        /// ws._04_InvokeMember = v.tWebInvokeMember.onchange;
        /// t.WebSet(webBrowser2, ws);
        /// ---
        /// ws._01_Caption = "Mevcut Ehliyet Belge No";
        /// ws._02_ElemanName = "txtEhliyetBelgeNo";
        /// ws._03_SetValue = EskiSertifikaBelgeSayisi;
        /// t.WebSet(webBrowser2, ws);
        /// ---
        /// ws._01_Caption = "Giriş Butonu";
        /// ws._02_ElemanName = "btnGiris";
        /// ws._04_InvokeMember = v.tWebInvokeMember.click;
        /// t.WebSet(webBrowser1, ws);


        public void WebSet(WebBrowser wb, tSetAttribute ws)
        {

            // Value atama işlemi 

            //if (webBrowser2.Document.GetElementById("txtCepTelefonNo") != null)
            //{
            //    webBrowser2.Document.GetElementById("txtCepTelefonNo").SetAttribute("value", CepTelefon);
            //    while (webBrowser2.ReadyState != WebBrowserReadyState.Complete)
            //           Application.DoEvents();
            //}

            // Atanan valueden sonra nesnenin tetiklenmesi gerekirse

            //webBrowser2.Document.GetElementById("cmbEgitimDonemi").InvokeMember("onchange");
            //while (webBrowser2.ReadyState != WebBrowserReadyState.Complete)
            //    Application.DoEvents();

            if (wb.Document == null) return;

            string inner = string.Empty;

            if (ws._04_InvokeMember != v.tWebInvokeMember.click)
            {
                if (ws._03_SetValue == "")
                {
                    MessageBox.Show("DİKKAT [error 1000] : [ " + ws._01_Caption + " ] için veri/bilgi yok...");
                    return;
                }

                //
                // mevcut value ile yeni gelen value eşit değilse işlem yapılcak
                //
                if (wb.Document.GetElementById(ws._02_ElemanName) != null)
                {
                    if (wb.Document.GetElementById(ws._02_ElemanName).GetAttribute("value") == ws._03_SetValue) return;
                }
            }

            if ((ws._03_SetValue != null) &&
                (wb.Document.GetElementById(ws._02_ElemanName) != null))
            {
                //
                // Value atama işlemi 
                //
                #region
                try
                {
                    wb.Document.GetElementById(ws._02_ElemanName).SetAttribute("value", ws._03_SetValue);
                    WebReadyComplate(wb);
                }
                catch (Exception exc1)
                {
                    inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                    WebReadyComplate(wb);
                    MessageBox.Show("DİKKAT [error 1001] : [ " + ws._01_Caption + " (" + ws._03_SetValue + ") ] veri ataması sırasında sorun oluştu ..." +
                        v.ENTER2 + inner);
                }
                #endregion
            }

            //
            // Atanan valueden sonra nesnenin tetiklenmesi gerekirse
            //
            #region
            if (ws._04_InvokeMember > v.tWebInvokeMember.none)
            {
                string invoke = string.Empty;
                if (ws._04_InvokeMember == v.tWebInvokeMember.click) invoke = "click";
                if (ws._04_InvokeMember == v.tWebInvokeMember.clickAndNewPage) invoke = "click";
                if (ws._04_InvokeMember == v.tWebInvokeMember.onchange) invoke = "onchange";
                if (ws._04_InvokeMember == v.tWebInvokeMember.submit) invoke = "submit";

                try
                {
                    WebReadyComplate(wb);
                    wb.Document.GetElementById(ws._02_ElemanName).InvokeMember(invoke);
                    WebReadyComplate(wb);
                }
                catch (Exception exc2)
                {
                    inner = (exc2.InnerException != null ? exc2.InnerException.ToString() : exc2.Message.ToString());

                    WebReadyComplate(wb);

                    MessageBox.Show("DİKKAT [error 1002] : [ " + ws._01_Caption + " (" + ws._03_SetValue + "), (" + invoke + ") ] verinin çalıştırılması sırasında sorun oluştu ..." +
                        v.ENTER2 + inner);
                }
            }
            #endregion

            //
            // Bir sonraki kullanım için değişkenleri temizle
            //
            ws._01_Caption = string.Empty;
            ws._02_ElemanName = string.Empty;
            ws._03_SetValue = string.Empty;
            ws._04_InvokeMember = v.tWebInvokeMember.none;

        }

        public void WebReadyComplate(WebBrowser wb)
        {
            while (wb.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();
        }

        public void ReadyComplate(int length)
        {
            for (int i = 0; i < length; i++)
            {
                Application.DoEvents();
            }
        }


        /// <summary>
        /// WebBrowser scroll position change 
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="ScrollPoint"></param>
        /// param = 1  Sayfa başı
        /// param = 2  Sayfa sonu
        /// param > 2  istenilen pixel    
        public void WebScrollPosition(WebBrowser wb, int ScrollPoint)
        {
            if (wb.Document != null)
            {
                while (wb.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();

                if (ScrollPoint > 2)
                {
                    Point scrollPoint = new Point(0, ScrollPoint);
                    wb.Document.Window.ScrollTo(scrollPoint);
                }

                if (ScrollPoint == 1)
                {
                    wb.Document.Body.ScrollIntoView(true);
                }

                if (ScrollPoint == 2)
                {
                    wb.Document.Body.ScrollIntoView(false);
                }
            }
        }

        #endregion WebBrowser

        #region Open Exe / Execute
        public void OpenExe(string ExeName, string file)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ExeName; // "GONDER.EXE";
            startInfo.Arguments = file;
            Process.Start(startInfo);
        }
        #endregion Open Exe / Execute

        #region Auto Close Exe / Execute
        public void LaunchCommandLineApp()
        {
            // For the example
            //const string ex1 = "C:\\";
            //const string ex2 = "C:\\Dir";

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = v.tExeAbout.activeExeName;   //"dcm2jpg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.Arguments = "-f j -o \"" + ex1 + "\" -z 1.0 -s y " + ex2;
            if (v.EXE_Arguments != "")
                startInfo.Arguments = v.EXE_Arguments;
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    v.SP_ApplicationExit = true;
                    //exeProcess.WaitForExit();
                    Application.Exit();
                }
            }
            catch
            {
                // Log error.
            }
        }
        #endregion Auto Close Exe / Execute


        #region Order
        public bool IsSearchControl(object sender)
        {
            bool onay = false;
            string typeName = "";
            string myProp = "";
            string tableIPCode = "";
            string senderType = sender.GetType().ToString();
            Form tForm = null;

            if (senderType == "DevExpress.XtraEditors.ButtonEdit")
            {
                tableIPCode = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleName;
                myProp = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleDescription;
                tForm = ((DevExpress.XtraEditors.ButtonEdit)sender).FindForm();
            }
            
            typeName = MyProperties_Get(myProp, "Type:");

            if (typeName == v.tSearch.searchEngine)
            {
                onay = true;

                // btArama / search işlemi başlasın
                if (myProp != "")
                {
                    PROP_NAVIGATOR prop_ = null;
                    List<PROP_NAVIGATOR> propList_ = null;
                    v.tButtonType propButtonType = v.tButtonType.btNone;

                    readProNavigator(myProp, ref prop_, ref propList_);

                    if (propList_ != null)
                    {
                        //tEvents ev = new tEvents();
                        tEventsButton evb = new tEventsButton();
                        foreach (PROP_NAVIGATOR item in propList_)
                        {
                            if (item.BUTTONTYPE.ToString() != "null")
                            {
                                propButtonType = getClickType(Convert.ToInt32(item.BUTTONTYPE.ToString()));
                            }
                            if (v.tButtonType.btArama == propButtonType)
                            {
                                /// Search Engine nin çalışıp çalışmayacağının kararı alınıyor
                                /// 
                                onay = tWorkingCheck(tForm, item, tableIPCode);
                                break;
                            }
                        }
                    }
                }
            }
            return onay;
        }
        public string tReadTextFile(string fileName)
        {
            //fname = v.EXE_ScriptsPath + "\\" + tableName + ".txt";

            string text = "";
            Boolean filenotfound = false;

            try
            {
                if (File.Exists(@"" + fileName))
                {
                    //FileStream fileStream = new FileStream(@"" + fname, FileMode.Open, FileAccess.Read);
                    
                    System.IO.TextReader readFile = new StreamReader(@"" + fileName);
                    text = readFile.ReadToEnd();
                    readFile.Close();
                    readFile = null;
                    filenotfound = false;
                }
                else filenotfound = true;

                if (filenotfound == true)
                {
                    MessageBox.Show("File not found : " + fileName);
                }
            }
            catch
            {
                text = "";
            }

            return text;
        }
        public string getFormName(string sourceFormCodeAndName)
        {
            string formCode = "";
            string formName = "";

            if (sourceFormCodeAndName.IndexOf("||") > -1)
            {
                formCode = Get_And_Clear(ref sourceFormCodeAndName, "||");
                formName = Get_And_Clear(ref sourceFormCodeAndName, "||");
            }
            return formName;
        }

        private void newExeUpdate(DataSet ds)
        {
            if (IsNotNull(ds) == false) return;

            /*
             * [COMP_KEY]
                   ,[ISACTIVE]
                   ,[REC_DATE]
                   ,[UPDATE_DATE]
                   ,[EXE_NAME]
                   ,[UPDATE_TD]
                   ,[UPDATE_NO]
                   ,[VERSION_NO]
                   ,[PACKET_NAME]
                   ,[ABOUT]
            */


            string text =
            @" <b>" + ds.Tables[0].Rows[0]["EXE_NAME"].ToString() + @"</b><br> Mevcut Versiyon : " + Application.ProductVersion.ToString() + @"<br> Yeni Versiyon : " + ds.Tables[0].Rows[0]["VERSION_NO"].ToString() + v.ENTER + @" ";

            AlertMessage("Güncelleme var... ", text);

            /// ftp : //ustadyazilim.com
            ///u8094836@edisonhost.com

            tFtp ftpClient = new tFtp(v.ftpHostIp, v.ftpUserName, v.ftpUserPass);
            //using ftpClient do

            //string[] simpleDirectoryListing = ftpClient.directoryListDetailed("/public");

            /* Download a File */
            //ftpClient.download("public/YesiLdefter_201806201.rar", @"C:\download\YesiLdefter_201806201.rar");


            ftpClient = null;
        }

        public void AlertMessage(string caption, string text)
        {
            //if (caption == "") caption = "Mesaj var... ";

            AlertInfo info = new AlertInfo(caption + v.ENTER, text);

            AlertControl control = new AlertControl();
            //control.
            control.AllowHtmlText = true;
            control.FormLocation = AlertFormLocation.BottomRight;
            control.Show(v.mainForm, info);
            //control.Show(v.mainForm, caption, text);
        }

        public void FlyoutMessage(Form tForm, string caption, string mesaj)
        {
            //SimpleButton button = new SimpleButton() { Text = "ShowFlyout" };
            //button.Dock = DockStyle.Top;
            //button.Parent = tForm;// ownerControl;

            if (v.IsWaitOpen)
            {
                v.IsWaitOpen = false;
                WaitFormClose();
            }

            FlyoutAction action = new FlyoutAction();
            action.Caption = caption;// "Flyout Action";
            action.Description = mesaj;// "Flyout Action Description";
            action.Commands.Add(FlyoutCommand.OK);

            if (tForm == null)
                FlyoutDialog.Show(v.mainForm, action);
            else FlyoutDialog.Show(tForm, action);
            //button.Click += (sender, e) => { FlyoutDialog.Show(ownerControl.FindForm(), action); };
            //button.Click += (sender, e) => { FlyoutDialog.Show(tForm, action); };
        }

        public void SelectPage(Form tForm, string mainControl, string pageName, int pageIndex)
        {
            Control c = Find_Control(tForm, mainControl);

            int i2 = 0;

            if (c != null)
            {
                #region TabPane
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane")
                {
                    if (IsNotNull(pageName))
                    {
                        i2 = ((DevExpress.XtraBars.Navigation.TabPane)c).Pages.Count;

                        for (int i = 0; i < i2; i++)
                        {
                            if (((DevExpress.XtraBars.Navigation.TabPane)c).Pages[i].Name == pageName)
                            {
                                ((DevExpress.XtraBars.Navigation.TabPane)c).SelectedPageIndex = i;
                                break;
                            }
                        }
                    }

                    if ((IsNotNull(pageName) == false) && (pageIndex > -1))
                    {
                        ((DevExpress.XtraBars.Navigation.TabPane)c).SelectedPageIndex = pageIndex;
                    }
                }
                #endregion TabPane

                #region NavigationPane
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
                {
                    if (IsNotNull(pageName))
                    {
                        i2 = ((DevExpress.XtraBars.Navigation.NavigationPane)c).Pages.Count;

                        for (int i = 0; i < i2; i++)
                        {
                            if (((DevExpress.XtraBars.Navigation.NavigationPane)c).Pages[i].Name == pageName)
                            {
                                ((DevExpress.XtraBars.Navigation.NavigationPane)c).SelectedPageIndex = i;
                                break;
                            }
                        }
                    }

                    if ((IsNotNull(pageName) == false) && (pageIndex > -1))
                    {
                        ((DevExpress.XtraBars.Navigation.NavigationPane)c).SelectedPageIndex = pageIndex;
                    }
                }
                #endregion NavigationPane

                #region XtraTabControl
                if (c.GetType().ToString() == "DevExpress.XtraTab.XtraTabControl")
                {
                    if (IsNotNull(pageName))
                    {
                        i2 = ((DevExpress.XtraTab.XtraTabControl)c).TabPages.Count;

                        for (int i = 0; i < i2; i++)
                        {
                            if (((DevExpress.XtraTab.XtraTabControl)c).TabPages[i].Name == pageName)
                            {
                                ((DevExpress.XtraTab.XtraTabControl)c).TabIndex = i;
                                break;
                            }
                        }
                    }

                    if ((IsNotNull(pageName) == false) && (pageIndex > -1))
                    {
                        ((DevExpress.XtraTab.XtraTabControl)c).TabIndex = pageIndex;
                    }
                }
                #endregion XtraTabControl

                #region BackstageViewControl
                if (c.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                {
                    if (IsNotNull(pageName))
                    {
                        selectBackstageViewTabItem(c, pageName, "???");
                    }

                    if ((IsNotNull(pageName) == false) && (pageIndex > -1))
                    {
                        ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).SelectedTabIndex = pageIndex;
                    }
                }
                #endregion BackstageViewControl
            }
        }

        public void selectBackstageViewTabItem(Control c, string pageName, string tabPageCode)
        {
            int i2 = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls.Count;
            if (tabPageCode == "") tabPageCode = "???";
            for (int i = 0; i < i2; i++)
            {
                if (((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls[i].Name == pageName || 
                    ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls[i].Name == tabPageCode)
                {
                    // sıfırda ekranda görmediğimiz bir nesne daha mevcut
                    ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).SelectedTabIndex = (i - 1);
                    break;
                }
            }
        }

        public void selectBackstageViewTabItem(Form tForm, string tabControlName, string pageName, string tabPageCode)
        {
            Control c = Find_Control(tForm, tabControlName);
            if (c == null) return;

            selectBackstageViewTabItem(c, pageName, tabPageCode);
        }
        public Control Find_BackstageViewTabItem(Form tForm, string tabControlName, string pageName)
        {
            Control c = Find_Control(tForm, tabControlName);
            if (c == null) return null;
            if (c.GetType().ToString() != "DevExpress.XtraBars.Ribbon.BackstageViewControl") return null;
            //DevExpress.XtraBars.Ribbon.BackstageViewTabItem
            Control item = null;

            int i2 = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls.Count;
            for (int i = 0; i < i2; i++)
            {
                if (((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls[i].Name == pageName) 
                {
                    item = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls[i];
                    break;
                }
            }

            return item;
        }

        public void Table_FieldsGroups_Count(vTableAbout vTA, DataSet dsFields)
        {
            // tableCount ? 1 = sadece fields bilgileri var, 
            //              2 = MS_Groups bilgileride var
            if (dsFields == null) return;

            int tableCount = 0;
            int fieldCount = 0;
            int groupCount = 0;

            /// ----- tespit yeri -----
            tableCount = dsFields.Tables.Count;
            if (tableCount > 0)
            {
                fieldCount = dsFields.Tables[0].Rows.Count;
                if (tableCount == 2)
                    groupCount = dsFields.Tables[1].Rows.Count;

            }// tableCount

            vTA.TablesCount = tableCount;
            vTA.FieldsCount = fieldCount;
            vTA.GroupsCount = groupCount;
        }

        public void Table_About(vTableAbout vTA, DataSet dsFields)
        {
            // tableCount ? 1 = sadece fields bilgileri var, 
            //              2 = MS_Groups bilgileride var
            if (dsFields == null) return;

            int tableCount = 0;
            int rowCount = 0;
            int colCount = 0;
            int groupCount = 0;
            int tGroupPanelCount = 0;
            int tDLTabPageCount = 0;
            int tTabPagesCount = 0;


            /// ----- TESPİT YERİ -----

            tableCount = dsFields.Tables.Count;
            if (tableCount > 0)
            {
                rowCount = dsFields.Tables[0].Rows.Count;
                colCount = dsFields.Tables[0].Columns.Count;
                if (tableCount == 2)
                    groupCount = dsFields.Tables[1].Rows.Count;

                #region groupCount
                if (groupCount > 0)
                {
                    if (dsFields.Tables[1].TableName == "GROUPS")
                    {
                        Int16 group_types = 0;
                        bool fvisible = false;

                        for (int i = 0; i < groupCount; i++)
                        {
                            /// GROUP_TYPES', 1, '', 'GroupPanel');
                            /// GROUP_TYPES', 2, '', 'DataLayoutTabPage');
                            /// GROUP_TYPES', 3, '', 'WelcomeWizard');
                            /// GROUP_TYPES', 4, '', 'WizardPage');
                            /// GROUP_TYPES', 5, '', 'CompletionWizard');
                            /// GROUP_TYPES', 6, '', 'TabPages');

                            group_types = Set(dsFields.Tables[1].Rows[i]["GROUP_TYPES"].ToString(), "", (Int16)0);
                            fvisible = Set(dsFields.Tables[1].Rows[i]["FVISIBLE"].ToString(), "", (bool)false);
                            if (fvisible)
                            {
                                if (group_types == 1) tGroupPanelCount++;
                                if (group_types == 2) tDLTabPageCount++;
                                if (group_types == 6) tTabPagesCount++;
                            }
                        } // for
                    } //if (dsFields.Tables[1].TableName == "GROUPS")
                }
                #endregion groupCount

            }// tableCount

            vTA.TablesCount = tableCount;
            vTA.FieldsCount = colCount;
            vTA.RecordCount = rowCount;
            vTA.GroupsCount = groupCount;
            vTA.groupPanelCount = tGroupPanelCount;
            vTA.dLTabPageCount = tDLTabPageCount;
            vTA.dWTabPageCount = tTabPagesCount;
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            // Eşzamansız işlem için benzersiz tanımlayıcı edinin. 
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            v.mailSent = true;
        }
        
        public void eMailSend(string[] args)
        {
            // Command line argument must the the SMTP host.
            // Komut satırı argümanı SMTP ana bilgisayar olmalıdır. 
            SmtpClient client = new SmtpClient(args[0]);

            // Specify the e-mail sender.
            // Create a mailing address that includes a UTF8 character
            // in the display name.
            // E-posta gönderenini belirtin. 
            // UTF8 karakteri içeren bir posta adresi oluşturun . 
            // Görünen adda 
            MailAddress from = new MailAddress("jane@contoso.com", "Jane " + (char)0xD8 + " Clayton", System.Text.Encoding.UTF8);

            // Set destinations for the e-mail message.
            // E-posta mesajının hedeflerini ayarlayın.
            MailAddress to = new MailAddress("ben@contoso.com");

            // Specify the message content.
            // Mesaj içeriğini belirtin.
            MailMessage message = new MailMessage(from, to);
            //message.Body = "This is a test e-mail message sent by an application. ";
            message.Body = "Bu, bir uygulama tarafından gönderilen bir test e-postası mesajıdır.";

            // Include some non-ASCII characters in body and subject.
            // Gövde ve öznede bazı ASCII olmayan karakterler ekleyin.
            string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
            message.Body += Environment.NewLine + someArrows;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = "test message 1" + someArrows;
            message.SubjectEncoding = System.Text.Encoding.UTF8;

            // Set the method that is called back when the send operation ends.
            // Gönderme işlemi sona erdiğinde geri çağrılan yöntemi ayarlayın.
            client.SendCompleted += new
            SendCompletedEventHandler(SendCompletedCallback);

            // The userState can be any object that allows your callback 
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            // UserState, geri arama 
            // yönteminizin bu gönderme işlemini tanımlamasına izin veren herhangi bir nesne olabilir . 
            // Bu örnekte, userToken bir dize sabitidir. 

            string userState = "test message1";
            client.SendAsync(message, userState);
            //Console.WriteLine("Sending message... press c to cancel mail. Press any other key to exit.");
            Console.WriteLine("Mesaj gönderme ... posta iptal etmek için c tuşuna basın, çıkmak için herhangi bir tuşa basın.");
            string answer = Console.ReadLine();

            // If the user canceled the send, and mail hasn't been sent yet,
            // then cancel the pending operation.
            // Kullanıcı gönderimi iptal etmişse ve posta henüz gönderilmemişse 
            // bekleyen işlemi iptal et. 

            if (answer.StartsWith("c") && v.mailSent == false)
            {
                client.SendAsyncCancel();
            }

            // Clean up.
            // Temizlemek.
            message.Dispose();
            Console.WriteLine("Goodbye.");
        }

        public void eMailSend2()
        {
            var message = new MailMessage("admin@bendytree.com", "someone@example.com");
            message.Subject = "What Up, Dog?";
            message.Body = "Why you gotta be all up in my grill?";
            SmtpClient mailer = new SmtpClient("smtp.gmail.com", 587);
            mailer.Credentials = new NetworkCredential("admin@bendytree.com", "YourPasswordHere");
            mailer.EnableSsl = true;
            mailer.Send(message);
        }

        public void eMailSend3(eMail eMail)
        {
            string userName = "tekinucar70@gmail.com";
            string userPass = "canberk98";
            string smtp = "smtp.gmail.com";
            string port = "587";
            string toMail = eMail.toMailAddress;     // gideceği adres
            string toCCMail = eMail.toCCMailAddress; // bilgi verilen mail
            string subject = eMail.subject;          // konu başlığı
            string bodyMessage = eMail.message;      // gönderilecek mesaj

            NetworkCredential login = new NetworkCredential(userName, userPass);

            SmtpClient client = new SmtpClient(smtp);
            client.Port = Convert.ToInt32(port);
            client.EnableSsl = true;
            client.Credentials = login;

            //MailMessage msg = new MailMessage { From = new MailAddress(userName + smtp.Replace("smtp.", "@"), "Ustad Bilişim", Encoding.UTF8) };
            MailMessage msg = new MailMessage { From = new MailAddress(userName, "Üstad Bilişim", Encoding.UTF8) };
            msg.To.Add(new MailAddress(toMail));

            if (!string.IsNullOrEmpty(toCCMail))
                msg.CC.Add(toCCMail);

            msg.Subject = subject;
            msg.Body = bodyMessage;
            msg.BodyEncoding = Encoding.UTF8;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.Normal;
            msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback3);

            string userState = "Sending...";
            client.SendAsync(msg, userState);

        }

        private void SendCompletedCallback3(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show(string.Format("{0} send canceled.", e.UserState), "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (e.Error != null)
                MessageBox.Show(string.Format("{0} {1}", e.UserState, e.Error), "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Mesaj başarıyla gönderilmiştir.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void eMailGet()
        {
            /*
            Almak biraz daha karmaşıktır. Üçüncü taraf bir kütüphaneye ihtiyaç duyuyor. 
            Google'ın GData API'sı ile çalışmaya çalıştım, ancak elimden gelen en iyi şey, gelen kutumun bir xml özet akışıydı. 
            Xml ayrıştırma yalnızca zamanı boşa harcamak isteyen insanlar içindir.

            3-4 başarısız kütüphaneden sonra OpenPOP'u çalıştım . GMail'i kullanıyorsanız, 
            gmail'in klasörleri ve POP'u olmadığı için IMAP'ı POP'dan daha iyi bulabilirsiniz. 

            Gelen kutunuzdaki en son e-postanın konusunu edinmek için gerekli olan kod:

            E-posta aldığımda daha fazla arama yapabilir ve sorgulayabilirim, ancak bu işi tamamlar.

            Bir sır bilmek ister misin? İşte, e-postayla almak istedim. Selenyum ile çalışan bazı otomasyonum var ve bazen bir captcha açılıyor. 
            
            Bu durumda bir ekran görüntüsü alıp kendim e-postayla gönderiyorum. Daha sonra, elverişli bir şekilde, e-postaya, captcha'da gördüğüm metinle cevap veriyorum. 
            
            Selenyumu içine koyar ve sürer. Kemik için epey kötü, ha?

            var client = new POPClient();
            client.Connect("pop.gmail.com", 995, true);
            client.Authenticate("admin@bendytree.com", "YourPasswordHere");
            var count = client.GetMessageCount();
            Message message = client.GetMessage(count);
            Console.WriteLine(message.Headers.Subject); 
              
            */


        }

        /// <summary>
        /// Verilen yoldaki resmi Byte array e çevirir ve return eder.
        /// </summary>
        /// <param name="CevrilecekResimYolu">Hangi resim dosyası üzerinde işlem yapılacak ise yolu.</param>
        /// <returns>byte[] e çevrilmiş şekilde resmi verir.</returns>
        public byte[] imageBinaryArrayConverter(string ImagesPath, ref long imageLength)
        {
            byte[] byteResim = null;
            FileInfo fInfo = new FileInfo(ImagesPath);
            long sayac = fInfo.Length;
            FileStream fStream = new FileStream(ImagesPath, FileMode.Open, FileAccess.Read);
            BinaryReader bReader = new BinaryReader(fStream);

            byteResim = bReader.ReadBytes((int)sayac);
            //v.con_Images_Length = byteResim.Length;
            imageLength = byteResim.Length;

            fStream.Close();
            fStream.Dispose();
            bReader.Close();
            bReader.Dispose();

            return byteResim;
        }

        public byte[] imageBinaryArrayConverterMem(byte[] tResim, ref long imageLength)
        {
            byte[] byteResim = null;
            long sayac = tResim.Length;
            MemoryStream mStream = new MemoryStream(tResim);
            BinaryReader bReader = new BinaryReader(mStream);

            byteResim = bReader.ReadBytes((int)sayac);
            imageLength = byteResim.Length;

            mStream.Close();
            mStream.Dispose();
            bReader.Close();
            bReader.Dispose();

            return byteResim;
        }

        public bool imageBinaryArrayConverterFile(byte[] tResim, string ImagesPath)
        {
            bool onay = false;
            try
            {
                byte[] byteResim = null;
                long sayac = tResim.Length;
                MemoryStream mStream = new MemoryStream(tResim);
                BinaryReader bReader = new BinaryReader(mStream);

                byteResim = bReader.ReadBytes((int)sayac);
                //imageLength = byteResim.Length;

                File.Delete(ImagesPath);
                //FileStream fStream = new FileStream(ImagesPath, FileMode.CreateNew, FileAccess.Write);
                File.WriteAllBytes(ImagesPath, byteResim);

                mStream.Close();
                mStream.Dispose();
                bReader.Close();
                bReader.Dispose();
                onay = true;
            }
            catch (Exception exc1)
            {
                string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                MessageBox.Show("DİKKAT [error image] : [ " + ImagesPath + " ] dosyasına veri ataması sırasında sorun oluştu ..." +
                    v.ENTER2 + inner);
                //throw;
            }

            return onay;
        }

        public Bitmap imageCompress(Bitmap workingImage, int newWidth, int newHeight)
        {
            Bitmap _img = new Bitmap(newWidth, newHeight, workingImage.PixelFormat);
            _img.SetResolution(workingImage.HorizontalResolution, workingImage.VerticalResolution);

            /// for new small image
            Graphics g = Graphics.FromImage(_img);
            /// create graphics
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            g.DrawImage(workingImage, 0, 0, newWidth, newHeight);

            return _img;
        }

        public void LookUpFieldNameChecked(
            ref string tableName,
            ref string fieldName,
            ref string idFieldName,
            Int16 fieldType
            )
        {
            string orjinalTableName = tableName;
            string orjinalFieldName = fieldName;

            if (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadMtsk)
            { 
                if (orjinalFieldName.IndexOf("KurumOnay") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "OnayTipi";
                    tableName = "MtskKurumOnayTipi";
                    return;
                }
                //if ((orjinalTableName.IndexOf("Mtsk") > -1) && (orjinalFieldName.IndexOf("DonemTipi") > -1))
                if (orjinalFieldName.IndexOf("DonemTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "DonemTipi";
                    tableName = "MtskDonemTipi";
                    return;
                }
                //if ((orjinalTableName.IndexOf("Mtsk") > -1) && (orjinalFieldName.IndexOf("GrupTipi") > -1))
                if ((orjinalFieldName.IndexOf("GrupTipi") > -1) &&
                    (orjinalFieldName.IndexOf("UstGrupTipi") == -1 &&
                     orjinalFieldName.IndexOf("AnaGrupTipi") == -1 && 
                     orjinalFieldName.IndexOf("AltGrupTipi") == -1))
                {
                    idFieldName = "Id";
                    fieldName = "GrupTipi";
                    tableName = "MtskGrupTipi";
                    return;
                }
                if (orjinalFieldName.IndexOf("SertifikaGrupTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "SertifikaGrupTipi";
                    tableName = "EduMtskDerslerSertifikaGrupTipi";
                    return;
                }
                //if ((orjinalTableName.IndexOf("Mtsk") > -1) && (orjinalFieldName.IndexOf("SubeTipi") > -1))
                if (orjinalFieldName.IndexOf("SubeTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "SubeTipi";
                    tableName = "MtskSubeTipi";
                    return;
                }
                if (orjinalFieldName.IndexOf("MevcutSertifikaTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "KSertifikaTipi";
                    tableName = "MtskSertifikaTipi";
                    return;
                }
                if (orjinalFieldName.IndexOf("SertifikaTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "SertifikaTipi";
                    tableName = "MtskSertifikaTipi";
                    return;
                }
                if (orjinalFieldName.IndexOf("Brans") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "BransTipi";
                    tableName = "MebBransTipi";
                    return;
                }
            }

            if (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadSrc)
            {
                if (orjinalFieldName.IndexOf("MevcutEhliyetTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "SertifikaTipi";
                    tableName = "MtskSertifikaTipi";
                    return;
                }

                if (orjinalFieldName.IndexOf("SertifikaTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "SertifikaTipi";
                    tableName = "SrcSertifikaTipi";
                    return;
                }

                if (orjinalFieldName.IndexOf("DonemTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "DonemTipi";
                    tableName = "SrcDonemTipi";
                    return;
                }
                if ((orjinalFieldName.IndexOf("GrupTipi") > -1) &&
                    (orjinalFieldName.IndexOf("UstGrupTipi") == -1 &&
                     orjinalFieldName.IndexOf("AnaGrupTipi") == -1 &&
                     orjinalFieldName.IndexOf("AltGrupTipi") == -1))
                {
                    idFieldName = "Id";
                    fieldName = "GrupTipi";
                    tableName = "SrcGrupTipi";
                    return;
                }
                if (orjinalFieldName.IndexOf("SubeTipi") > -1)
                {
                    idFieldName = "Id";
                    fieldName = "SubeTipi";
                    tableName = "SrcSubeTipi";
                    return;
                }





            }


            if (fieldName.IndexOf("UlkeKodu") > -1)
            {
                idFieldName = "Id";
                fieldName = "UlkeKodu";
                tableName = "UlkeTipi";
                return;
            }
            if (fieldName.IndexOf("UlkeTipiId") > -1)
            {
                idFieldName = "Id";
                fieldName = "UlkeKodu";
                tableName = "UlkeTipi";
                return;
            }
            if (fieldName.IndexOf("Cinsiyet") > -1)
            {
                idFieldName = "Id";
                fieldName = "CinsiyetTipi";
                tableName = "CinsiyetTipi";
                return;
            }
            if ((fieldName.IndexOf("IlKodu") > -1) ||
                (fieldName.IndexOf("City") > -1))
            {
                idFieldName = "IlKodu";
                fieldName = "IlAdiBUYUK";
                tableName = "ILTipi";
                return;
            }
            if ((fieldName.IndexOf("IlceKodu") > -1) ||
                (fieldName.IndexOf("District") > -1))
            {
                idFieldName = "IlceKodu";
                fieldName = "IlceAdi";
                tableName = "IlceTipi";
                return;
            }

            if (fieldName.IndexOf("KdvOrani") > -1)
            {
                idFieldName = "Id";
                fieldName = "KdvOraniAdi";
                tableName = "OnmKdvOraniTipi";
                return;
            }
            if (fieldName.IndexOf("Para") > -1)
            {
                idFieldName = "ParaKodu";
                fieldName = "ParaAdi";
                tableName = "OnmParaTipi";
                return;
            }
            if (fieldName.IndexOf("BirimTipi") > -1)
            {
                idFieldName = "BirimKodu";
                fieldName = "BirimAdi";
                tableName = "OnmStokBirimTipi";
                return;
            }

            if (fieldName.ToUpper().IndexOf("SECTOR") > -1)
            {
                // (MsProjectTablesSectorTypeId ve
                //  MsProjectProceduresSectorTypeId ve
                //  MsProjectFunctionsSectorTypeId ) = MsSectorType
                //  veya 
                //  UstadFirmsSectorTypeId gibi
                idFieldName = "Id";
                fieldName = "SectorType";
                tableName = "MsSectorType";
                return;
            }
            if (tableName == "HubBildirimSablonlari")
            {
                /// değişikliğe gerek yok
                return;
            }

            /// BelgeTipiId >> BelgeTipi ne dönüşüyor 
            /// ParaTipiId  >> ParaTipi
            /// ILTipiId    >> ILTipi
            /// ILCETipiId  >> ILCETipi
            ///
            fieldName = fieldName.Replace("Id", "");
            if (tableName.IndexOf(fieldName) == -1)
                tableName = tableName + fieldName;

            if ((fieldType == 167) || // varchar
                (fieldType == 231))   // nvarchar
            {
                fieldName = fieldName.Replace("Tipi", "");
                fieldName = fieldName.Replace("Type", "");

                // IslemKodu oluştu
                idFieldName = fieldName + "Kodu";
                // IslemAdi oluştu
                fieldName = fieldName + "Adi";

                if (fieldName.IndexOf("Para") > -1)
                    tableName = "OnmParaTipi";
            }
        }

        public DataSet getTabimValues(string tableName, string where)
        {
            tSQLs sqls = new tSQLs();
            String Sql = "";
            DataSet ds = new DataSet();

            v.active_DB.runDBaseNo = v.dBaseNo.Local;

            Sql = sqls.SQL_TabimValues(tableName, where);

            SQL_Read_Execute(v.dBaseNo.Local, ds, ref Sql, "", "getTabimValues");
            
            return ds;
        }

        #endregion Order

        #region ftp Connection
        public void ftpDownloadIniFile()
        {
            bool onay = false;
            
            onay = ftpDownload(v.tExeAbout.activePath, "YesiLdefterConnection.Ini");
            
            string MainManagerDbUses = "";
            string SourceDbUses = "";

            v.active_DB.mainManagerDbUses = false;
            
            // Normal kullanıcılar için
            var ConnectionIni = new tIniFile("YesiLdefterConnection.Ini");
            if (ConnectionIni != null)
            {
                v.active_DB.managerServerName = ConnectionIni.Read("PublishManagerServerIp"); 
                v.active_DB.managerDBName = ConnectionIni.Read("PublishManagerDbName");
                v.active_DB.ustadCrmServerName = ConnectionIni.Read("UstadCrmServerIp");
                v.active_DB.ustadCrmDBName = ConnectionIni.Read("UstadCrmDbName");
                v.publishManager_DB.serverName = ConnectionIni.Read("PublishManagerServerIp");
                v.publishManager_DB.databaseName = ConnectionIni.Read("PublishManagerDbName");
            }

            // Ustad çalışanları için 
            if (File.Exists("YesiLdefter.Ini"))
            {
                File.Delete("YesiLdefter.Ini");
            }
            
            var YesiLdefterIni = new tIniFile("YesiLdefter2.Ini");
            MainManagerDbUses = YesiLdefterIni.Read("MainManagerDbUses");
            if (MainManagerDbUses.ToUpper() == "TRUE")
            {
                v.active_DB.mainManagerDbUses = true;
                v.active_DB.managerServerName = YesiLdefterIni.Read("MainManagerServerIp");
                v.active_DB.managerDBName = YesiLdefterIni.Read("MainManagerDbName");
                v.active_DB.ustadCrmServerName = ConnectionIni.Read("UstadCrmServerIp");
                v.active_DB.ustadCrmDBName = ConnectionIni.Read("UstadCrmDbName");
                v.publishManager_DB.serverName = ConnectionIni.Read("PublishManagerServerIp");
                v.publishManager_DB.databaseName = ConnectionIni.Read("PublishManagerDbName");

                v.active_DB.masterServerName = YesiLdefterIni.Read("MasterServerIp");
                v.active_DB.masterDBName = "master";
                v.active_DB.masterUserName = YesiLdefterIni.Read("MasterLoginName");
                v.active_DB.masterPsw = YesiLdefterIni.Read("MasterDbPass");
                if (IsNotNull(v.active_DB.masterPsw))
                    v.active_DB.masterPsw = " Password = " + v.active_DB.masterPsw + "; ";

                /// Eğer kullanıcı YesiLdefter2.Ini yi kullanıyorsa tester kullanıcıdır
                /// 
                v.SP_tUserType = v.tUserType.TesterUser;
            }

            // Tabim Surucu07 için 
            var YesiLdefterTabimIni = new tIniFile("YesiLdefterTabim.Ini");
            SourceDbUses = YesiLdefterTabimIni.Read("SourceDbUses");
            if (SourceDbUses.ToUpper() == "TRUE")
            {
                if (v.SP_TabimParamsKurumTipi == "")
                    v.SP_TabimParamsKurumTipi = "MTSK";

                if (MainManagerDbUses.ToUpper() == "TRUE")
                {
                    v.active_DB.managerServerName = YesiLdefterIni.Read("MainManagerServerIp");
                    v.active_DB.managerDBName = YesiLdefterIni.Read("MainManagerDbName");
                }
                else
                {
                    v.active_DB.managerServerName = ConnectionIni.Read("PublishManagerServerIp");
                    v.active_DB.managerDBName = ConnectionIni.Read("PublishManagerDbName");
                }
                
                v.active_DB.ustadCrmServerName = ConnectionIni.Read("UstadCrmServerIp");
                v.active_DB.ustadCrmDBName = ConnectionIni.Read("UstadCrmDbName");

                v.publishManager_DB.serverName = ConnectionIni.Read("PublishManagerServerIp");
                v.publishManager_DB.databaseName = ConnectionIni.Read("PublishManagerDbName");

                v.active_DB.localDBType = v.dBaseType.MSSQL;
                v.active_DB.localServerName = YesiLdefterTabimIni.Read("SourceServerNameIP");
                v.active_DB.localDBName = YesiLdefterTabimIni.Read("SourceDatabaseName");
                v.active_DB.localUserName = YesiLdefterTabimIni.Read("SourceDbLoginName");
                v.active_DB.localPsw = YesiLdefterTabimIni.Read("SourceDbPass");

                if (v.SP_TabimParamsKurumTipi == "SRC")
                {
                    v.active_DB.localServerName = YesiLdefterTabimIni.Read("SRCServerNameIP");
                    v.active_DB.localDBName = YesiLdefterTabimIni.Read("SRCDatabaseName");
                    v.active_DB.localUserName = YesiLdefterTabimIni.Read("SRCDbLoginName");
                    v.active_DB.localPsw = YesiLdefterTabimIni.Read("SRCDbPass");
                }

                // Exe açılış Params ile farklı bir ServerName geldiyse
                if ((v.SP_TabimParamsServerName != "") &&
                    (v.SP_TabimParamsServerName != v.active_DB.localServerName))
                {
                    v.active_DB.localServerName = v.SP_TabimParamsServerName;
                    v.active_DB.projectServerName = v.SP_TabimParamsServerName;
                }

                /// YesiLdefterTabim.Ini var içeriği null geliyorsa
                if (v.active_DB.localServerName == "" || v.active_DB.localServerName == "null")
                {
                    onay = readTABSURUCU_INI();
                    if (onay)
                    {
                        v.active_DB.projectServerName = v.active_DB.localServerName;
                        onay = writeYesiLdefterTabimIni();
                        if (onay == false)
                        {
                            FlyoutMessage(null, "Local server name", "Gerekli olan MSSQL bağlantı bilgisi YesiLdefterTabim.Ini yazılamadı. Yinede program çalışmaz ise TABİM DESTEK MASASI ndan yardım isteyin...");
                            Application.Exit();
                        }
                    }
                    else
                    {
                        FlyoutMessage(null, "Local server name", "Gerekli olan MSSQL bağlantı bilgisine ulaşamadım. Programı yeniden çalıştırmayı deneyin. Yinede program çalışmaz ise TABİM DESTEK MASASI ndan yardım isteyin...");
                        Application.Exit();
                    }
                }

                // ini file içinde manuel false yapılmış olabilir
                // yapılırsa sonuç ne olur bilmiyorum
                v.active_DB.localDbUses = true;
                v.SP_TabimDbConnection = Convert.ToBoolean(YesiLdefterTabimIni.Read("SourceConnection"));

                if (v.SP_TabimDbConnection)
                {
                    if ((IsNotNull(v.active_DB.localServerName.ToUpper()) == false) ||
                        (IsNotNull(v.active_DB.localDBName.ToUpper()) == false) ||
                        (IsNotNull(v.active_DB.localUserName.ToUpper()) == false) ||
                        (IsNotNull(v.active_DB.localPsw.ToUpper()) == false))
                    {
                        v.tUser.UserId = 0;
                        // ilk çalıştımada boş geliyor ise
                        if (IsNotNull(v.active_DB.localDBName) == false)
                        {
                            v.active_DB.localDBName = "Surucu07";
                            if (v.SP_TabimParamsKurumTipi == "SRC")
                                v.active_DB.localDBName = "SRC07";
                        }
                        if (IsNotNull(v.active_DB.localUserName) == false) v.active_DB.localUserName = "TABIM";
                        if (IsNotNull(v.active_DB.localPsw) == false) v.active_DB.localPsw = "312";

                        if (IsNotNull(v.active_DB.localServerName))
                        {
                            preparingLocalDbConnectionText();
                            v.SP_TabimIniWrite = true;
                        }
                    }
                    else
                    {
                        preparingLocalDbConnectionText();
                    }
                }
            }

        }
        
        public bool readTABSURUCU_INI()
        {
            bool onay = true;
            var tabSurucuIni = new tIniFile("c:\\Windows\\TABSURUCU.INI");
            if (tabSurucuIni != null)
            {
                v.active_DB.localServerName = tabSurucuIni.Read("cbServer_Text", "TfrmLoginDB");
                v.active_DB.localUserName = tabSurucuIni.Read("edUsername_Text", "TfrmLoginDB");
                v.active_DB.localPsw = tabSurucuIni.Read("edPassword_Text", "TfrmLoginDB");

                if (v.active_DB.localUserName == "") v.active_DB.localUserName = "TABIM";
                if (v.active_DB.localPsw == "") v.active_DB.localPsw = "312";
                if (IsNotNull(v.active_DB.localDBName) == false) v.active_DB.localDBName = "Surucu07";

                if (v.active_DB.localServerName == "")
                    v.active_DB.localServerName = Find_ListAvailableMSSQLServers();
            }
            return onay;
        }

        public bool writeYesiLdefterTabimIni()
        {
            bool onay = true;

            if ((IsNotNull(v.active_DB.localServerName) == false) ||
                (IsNotNull(v.active_DB.localDBName) == false) ||
                (IsNotNull(v.active_DB.localUserName) == false) ||
                (IsNotNull(v.active_DB.localPsw) == false))
            {
                AlertMessage("YesiLdefterTabim.Ini", "Bağlantı bilgilerinde eksiklik mevcut olduğu için Ini dosyasına yazılamadı...");
                return false;
            }

            try
            {
                var YesiLdefterTabimIni = new tIniFile("YesiLdefterTabim.Ini");
                if (YesiLdefterTabimIni != null)
                {
                    if (v.SP_TabimParamsKurumTipi == "MTSK")
                    {
                        YesiLdefterTabimIni.Write("SourceServerNameIP", v.active_DB.localServerName, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SourceDatabaseName", v.active_DB.localDBName, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SourceDbLoginName", v.active_DB.localUserName, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SourceDbPass", v.active_DB.localPsw, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SourceConnection", "true", "YesiLDefter");
                    }
                    if (v.SP_TabimParamsKurumTipi == "SRC")
                    {
                        YesiLdefterTabimIni.Write("SRCServerNameIP", v.active_DB.localServerName, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SRCDatabaseName", v.active_DB.localDBName, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SRCDbLoginName", v.active_DB.localUserName, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SRCDbPass", v.active_DB.localPsw, "YesiLDefter");
                        YesiLdefterTabimIni.Write("SourceConnection", "true", "YesiLDefter");
                    }
                    if (v.SP_TabimParamsKurumTipi == "ISMAK")
                    {
                        MessageBox.Show("ISMAK writeTabSurucuIni bak");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("DİKKAT : YesiLdefterTabim.Ini dosyasına kayıt işlemi gerçekleşmedi...");
                onay = false;
                //throw;
            }

            return onay;
        }

        public bool ftpDownload(string path, string fileName)
        {
            bool onay = false;

            tFtp ftpClient = null;

            /* Create Object Instance */
            ftpClient = new tFtp(v.ftpHostIp, v.ftpUserName, v.ftpUserPass);

            try
            {
                /// download sırasında hata oluşuyorsa dasya ismindeki büyük/küçük harfe dikkat et
                /// YesiLdefter2.Ini  hatalı
                /// YesiLdefter2.ini  çalışıyor 
                /// 
                onay = ftpClient.download(fileName, @"" + path + "\\" + fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ftp download" + v.ENTER2 + ex.Message + v.ENTER2 + "FileName : " + fileName);
                //throw;
            }
            /* Release Resources */
            ftpClient = null;

            if (onay)
            {
                v.Kullaniciya_Mesaj_Var = "Download gerçekleşti ...";
                v.timer_Kullaniciya_Mesaj_Var_.Start();
            }
            return onay;
        }
        public bool ftpTesterDownload(string path, string fileName)
        {
            bool onay = false;

            tFtp ftpClient = null;

            /* Create Object Instance */
            ftpClient = new tFtp(v.ftpHostIp, v.ftpTesterUserName, v.ftpUserPass);

            try
            {
                onay = ftpClient.download(fileName, @"" + path + "\\" + fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ftp download" + v.ENTER2 + ex.Message);
                //throw;
            }
            /* Release Resources */
            ftpClient = null;

            if (onay)
            {
                v.Kullaniciya_Mesaj_Var = "Download gerçekleşti ...";
                v.timer_Kullaniciya_Mesaj_Var_.Start();
            }
            return onay;
        }

        public bool ftpUpload(exeAbout exeFileAbout)
        {
            bool onay = false;

            tFtp ftpClient = null;

            try
            {
                /* Create Object Instance */
                ftpClient = new tFtp(v.ftpHostIp, v.ftpUserName, v.ftpUserPass);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Hata : Ftp bağlantı problemi ..." + v.ENTER2 + e1.Message);
                //throw;
            }

            try
            {
                /* Upload a File */
                ftpClient.upload(exeFileAbout.newPacketName, exeFileAbout.activePath + "\\" + exeFileAbout.newPacketName);
                onay = true;
            }
            catch (Exception e2)
            {
                MessageBox.Show("Hata : Ftp upload (yükleme) problemi ..." + v.ENTER2 + e2.Message);
                //throw;
            }

            /* Release Resources */
            ftpClient = null;

            if (onay)
            {
                MessageBox.Show(":)  [  " + exeFileAbout.newPacketName + "  ]  paket ftp ye yüklendi ...");
            }

            return onay;
        }

        public bool ftpTesterUpload(exeAbout exeFileAbout)
        {
            bool onay = false;

            tFtp ftpClient = null;

            try
            {
                /* Create Object Instance */
                ftpClient = new tFtp(v.ftpHostIp, v.ftpTesterUserName, v.ftpUserPass);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Hata : Ftp bağlantı problemi ..." + v.ENTER2 + e1.Message);
            }

            try
            {
                /* Upload a File */
                ftpClient.upload(exeFileAbout.newPacketName, exeFileAbout.activePath + "\\" + exeFileAbout.newPacketName);
                onay = true;
            }
            catch (Exception e2)
            {
                MessageBox.Show("Hata : Ftp upload (yükleme) problemi ..." + v.ENTER2 + e2.Message);
                //throw;
            }

            /* Release Resources */
            ftpClient = null;

            if (onay)
            {
                MessageBox.Show(":)  [  " + exeFileAbout.newPacketName + "  ]  paket 'home/webadmin/web/ustadyazilim.com/desktoptester'  yüklendi ...");
            }

            return onay;
        }

        #endregion ftp

        #region CompressFile
        public bool CompressFile(exeAbout exeFileAbout, v.fileType fileType)
        {
            if (exeFileAbout is null)
            {
                //throw new ArgumentNullException(nameof(exeAbout_));
                return false;
            }

            bool onay = false;
            DirectoryInfo di = new DirectoryInfo(exeFileAbout.activePath);

            v.tExeAbout.FileType = fileType;

            string fileName = "";
            
            if (fileType == v.fileType.ActiveExe) fileName = exeFileAbout.activeExeName;
            if (fileType == v.fileType.OrderFile) fileName = exeFileAbout.orderFileName;
            
            foreach (FileInfo fi in di.GetFiles())
            {
                //for specific file 
                if (fi.ToString() == fileName)
                {
                    onay = CompressFile_(fi, exeFileAbout);
                    break;
                }
            }

            if (exeFileAbout.orderFileExtension == "DIR" || exeFileAbout.orderFileExtension == "Directory")
            {
                foreach (DirectoryInfo dinfo in di.GetDirectories())
                {
                    
                    if (dinfo.ToString() == fileName)
                    {
                        onay = CompressDIR_(dinfo, exeFileAbout);
                        break;
                    }
                }
            }

            if (onay)
            {
                MessageBox.Show(":)  [  " + exeFileAbout.newPacketName + "  ]  paketlendi ...");
            }

            return onay;
        }
        private bool CompressFile_(FileInfo fi, exeAbout exeFileAbout)
        {
            bool onay = false;

            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and 
                // already compressed files.
                if ((File.GetAttributes(fi.FullName)
                    & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".gz")
                {
                    //string myFileName = fi.FullName + ".gz";
                    string myFileName = fi.FullName.Remove(fi.FullName.IndexOf(fi.Extension), fi.Extension.Length) + ".gz";

                    //exe kendisini compress yapacaksa
                    if (exeFileAbout.FileType == v.fileType.ActiveExe)
                        myFileName = preparingExeFileName(fi, exeFileAbout);

                    if (exeFileAbout.FileType == v.fileType.OrderFile)
                        myFileName = preparingOrderFileName(fi, exeFileAbout);

                    // çalışan kod
                    // Create the compressed file.
                    using (FileStream outFile = File.Create(myFileName))
                    {
                        using (GZipStream Compress = new GZipStream(outFile, CompressionMode.Compress))
                        {
                            // Copy the source file into 
                            // the compression stream.
                            inFile.CopyTo(Compress);

                            onay = true;

                            //Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                            //    fi.Name, fi.Length.ToString(), outFile.Length.ToString());
                        }
                    }

                }
            }

            return onay;
        }
        private string preparingExeFileName(FileInfo fi, exeAbout exeFileAbout)
        {
            string myFileName = "";

            if (fi.FullName.IndexOf(exeFileAbout.activeExeName) > -1)
            {
                /// activeVersionNo : 20190329_1545
                /// activeFileName  : YesiLdefter.exe >> YesiLdefter_20190329_1545.gz  şeklinde olacak
                /// activePath      : E:\TekinOzel\yesiLdefter\yesiLdefterV3\YesiLdefter\bin\Debug\YesiLdefter_20190329_1553.gz
                /// dikkat : kafan karışmasın şuan çalıştığın exeyi ftpye atmak istiyorsun
                /// bu nedenle active olandan faydalanıyor onu new diye ftp göndeririyoruz

                exeFileAbout.newVersionNo = exeFileAbout.activeVersionNo;
                exeFileAbout.newFileName = exeFileAbout.activeExeName;
                exeFileAbout.newPacketName =
                    exeFileAbout.activeExeName.Remove(exeFileAbout.activeExeName.IndexOf(fi.Extension), fi.Extension.Length) + "_"
                  + exeFileAbout.activeVersionNo + ".gz";

                // bu da aynı sonucu veriyor 
                //fi.FullName.Remove(fi.FullName.IndexOf(fi.Extension), fi.Extension.Length) + "_" + v.tExeAbout.activeVersionNo + ".gz";

                myFileName = exeFileAbout.activePath + "\\" + exeFileAbout.newPacketName;
            }

            return myFileName;
        }
        private string preparingOrderFileName(FileInfo fi, exeAbout exeFileAbout)
        {
            string myFileName = "";

            if (fi.FullName.IndexOf(exeFileAbout.orderFileName) > -1)
            {
                /// orderVersionNo : 20190329_1545
                /// orderFileName  : YesiLdefter.exe >> YesiLdefter_20190329_1545.gz  şeklinde olacak
                /// activePath      : E:\TekinOzel\yesiLdefter\yesiLdefterV3\YesiLdefter\bin\Debug\YesiLdefter_20190329_1553.gz
                /// dikkat : kafan karışmasın şuan exenin çalıştığın path içindeki dosyayı  ftpye atmak istiyorsun
                /// bu nedenle mevcut olan dosyadan faydalanıyor onu new diye ftp göndeririyoruz

                exeFileAbout.newVersionNo = exeFileAbout.orderFileVersionNo;
                exeFileAbout.newFileName = exeFileAbout.orderFileName;
                exeFileAbout.newPacketName =
                    exeFileAbout.orderFileName.Remove(exeFileAbout.orderFileName.IndexOf(fi.Extension), fi.Extension.Length) + "_"
                  + exeFileAbout.orderFileVersionNo.Replace('.', '_') + "_" + fi.Extension.Replace(".","") + ".gz";

                // bu da aynı sonucu veriyor 
                //fi.FullName.Remove(fi.FullName.IndexOf(fi.Extension), fi.Extension.Length) + "_" + v.tExeAbout.activeVersionNo + ".gz";

                myFileName = exeFileAbout.activePath + "\\" + exeFileAbout.newPacketName;
            }

            return myFileName;
        }
        
        private bool CompressDIR_(DirectoryInfo di, exeAbout exeFileAbout)
        {
            bool onay = false;

            string myPacketName = "";
            if (exeFileAbout.FileType == v.fileType.OrderFile)
                myPacketName = preparingOrderDIRName(di, exeFileAbout);

            string path = "";
            if (exeFileAbout.orderFilePath != null)
                path = exeFileAbout.orderFilePath;
            else path = exeFileAbout.activePath;
            
            path += "\\" + exeFileAbout.orderFileName;

            try
            {
                // Klasörü ZIP'le
                ZipFile.CreateFromDirectory(path, myPacketName);
                onay = true;
                MessageBox.Show("Klasör başarıyla ZIP'lendi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }

            return onay;
        }

        private string preparingOrderDIRName(DirectoryInfo di, exeAbout exeFileAbout)
        {
            string myFileName = "";

            if (di.FullName.IndexOf(exeFileAbout.orderFileName) > -1)
            {
                /// orderVersionNo : 20190329_1545
                /// orderFileName  : x64 klasörü >> x64_v250224.zip  şeklinde olacak
                /// activePath      : E:\TekinOzel\yesiLdefter\yesiLdefterV3\YesiLdefter\bin\Debug\x64_v250224.zip
                /// dikkat : kafan karışmasın şuan exenin çalıştığın path içindeki bir klasörü  ftpye atmak istiyorsun
                /// bu nedenle mevcut olan dosyadan faydalanıyor onu new diye ftp göndeririyoruz

                exeFileAbout.newVersionNo = exeFileAbout.orderFileVersionNo;
                exeFileAbout.newFileName = exeFileAbout.orderFileName;
                exeFileAbout.newPacketName =
                    exeFileAbout.orderFileName + "_"
                  + exeFileAbout.orderFileVersionNo.Replace('.', '_') + ".zip";

                // bu da aynı sonucu veriyor 
                //fi.FullName.Remove(fi.FullName.IndexOf(fi.Extension), fi.Extension.Length) + "_" + v.tExeAbout.activeVersionNo + ".gz";

                myFileName = exeFileAbout.activePath + "\\" + exeFileAbout.newPacketName;
            }

            return myFileName;
        }


        #endregion CompressFile

        #region Tabim Meb Aday/Sertifika Count

        public void TabimMebAdayCountWrite()
        {
            tSQLs Sqls = new tSQLs();
            DataSet ds_QueryA = new DataSet();
            DataSet ds_QueryAS = new DataSet();
            string Sql = "";
            string SqlCumlesi = "";
            try
            {
                Sql = Sqls.Sql_TabimMebAdayCount();
                SQL_Read_Execute(v.dBaseNo.Local, ds_QueryA, ref Sql, "FirmACount", "FirmACount");

                if (IsNotNull(ds_QueryA))
                {
                    SqlCumlesi = "";
                    int satir = ds_QueryA.Tables[0].Rows.Count;
                    for (int i = 0; i < satir; i++)
                    {
                        SqlCumlesi += ds_QueryA.Tables[0].Rows[i]["SqlCumlesi"].ToString() + v.ENTER;
                    }

                    SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_QueryA, ref SqlCumlesi, "FirmACount", "FirmACount");
                }
            }
            catch (Exception)
            {
                //throw;
            }
            try
            {
                Sql = Sqls.Sql_TabimMebAdaySertifikaCount();
                SQL_Read_Execute(v.dBaseNo.Local, ds_QueryAS, ref Sql, "FirmASCount", "FirmASCount");

                if (IsNotNull(ds_QueryAS))
                {
                    SqlCumlesi = "";
                    int satir = ds_QueryAS.Tables[0].Rows.Count;
                    for (int i = 0; i < satir; i++)
                    {
                        SqlCumlesi += ds_QueryAS.Tables[0].Rows[i]["SqlCumlesi"].ToString() + v.ENTER;
                    }

                    SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_QueryAS, ref SqlCumlesi, "FirmASCount", "FirmASCount");
                }
            }
            catch (Exception)
            {
                //throw;
            }
        }

        #endregion Tabim Meb Aday/Sertifika Count
    }


    #region 
    /*


Select a.[name]
 , a.[object_id]
 , b.[name] [param_name]
 , b.[parameter_id]
 , b.[system_type_id]
 , b.[user_type_id]
 , b.[max_length]

from [sys].[procedures] a
, [sys].[parameters] b
where a.[object_id] = b.[object_id] 


// Shows the open file dialog, and opens the file(s) specified by the user, if any.
    private void Open()
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        {
            dialog.Filter = "Xml Documents (*.xml)|*.xml|All Files (*.*)|*.*";
            dialog.Multiselect = true;
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.Open(dialog.FileNames);
            }
        }
    }

bilgisayar üzerinde çalışan diğer programların listesi

            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                //process
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    Console.WriteLine("Process: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
                }
            }





*/
    #endregion

    #region
    /*
    foreach (Control c in tForm.Controls)
    {

        if (Control_(c, Name, AccessibleName, ControlType, i)) return c;

        if (c.Controls.Count > 0)
        {
            #region Control2
            foreach (Control c2 in c.Controls)
            {
                if (Control_(c2, Name, AccessibleName, ControlType, i)) return c2;

                if (c2.Controls.Count > 0)
                {
                    #region Control3
                    foreach (Control c3 in c2.Controls)
                    {
                        if (Control_(c3, Name, AccessibleName, ControlType, i)) return c3;

                        if (c3.Controls.Count > 0)
                        {
                            #region Control4
                            foreach (Control c4 in c3.Controls)
                            {
                                if (Control_(c4, Name, AccessibleName, ControlType, i)) return c4;

                                if (c4.Controls.Count > 0)
                                {
                                    #region Control5
                                    foreach (Control c5 in c4.Controls)
                                    {
                                        if (Control_(c5, Name, AccessibleName, ControlType, i)) return c5;

                                        if (c5.Controls.Count > 0)
                                        {
                                            #region Control6
                                            foreach (Control c6 in c5.Controls)
                                            {
                                                if (Control_(c6, Name, AccessibleName, ControlType, i)) return c6;

                                                if (c6.Controls.Count > 0)
                                                {
                                                    #region Control7
                                                    foreach (Control c7 in c6.Controls)
                                                    {
                                                        if (Control_(c7, Name, AccessibleName, ControlType, i)) return c7;

                                                        if (c7.Controls.Count > 0)
                                                        {
                                                            #region Control8
                                                            foreach (Control c8 in c7.Controls)
                                                            {
                                                                if (Control_(c8, Name, AccessibleName, ControlType, i)) return c8;

                                                                if (c8.Controls.Count > 0)
                                                                {
                                                                    #region Control9
                                                                    foreach (Control c9 in c8.Controls)
                                                                    {
                                                                        if (Control_(c9, Name, AccessibleName, ControlType, i)) return c9;

                                                                        if (c9.Controls.Count > 0)
                                                                        {
                                                                            #region Control10
                                                                            foreach (Control c10 in c9.Controls)
                                                                            {
                                                                                if (Control_(c10, Name, AccessibleName, ControlType, i)) return c10;

                                                                                if (c10.Controls.Count > 0)
                                                                                {
                                                                                    #region Control11
                                                                                    foreach (Control c11 in c10.Controls)
                                                                                    {
                                                                                        if (Control_(c11, Name, AccessibleName, ControlType, i)) return c11;

                                                                                        if (c11.Controls.Count > 0)
                                                                                        {
                                                                                            #region Control12
                                                                                            foreach (Control c12 in c11.Controls)
                                                                                            {
                                                                                                if (Control_(c12, Name, AccessibleName, ControlType, i)) return c12;

                                                                                                if (c12.Controls.Count > 0)
                                                                                                {
                                                                                                    #region Control13
                                                                                                    foreach (Control c13 in c12.Controls)
                                                                                                    {
                                                                                                        if (Control_(c13, Name, AccessibleName, ControlType, i)) return c13;

                                                                                                        if (c13.Controls.Count > 0)
                                                                                                        {
                                                                                                            #region Control14
                                                                                                            foreach (Control c14 in c13.Controls)
                                                                                                            {
                                                                                                                if (Control_(c14, Name, AccessibleName, ControlType, i)) return c14;

                                                                                                                if (c14.Controls.Count > 0)
                                                                                                                {
                                                                                                                    #region Control15
                                                                                                                    foreach (Control c15 in c14.Controls)
                                                                                                                    {
                                                                                                                        if (Control_(c15, Name, AccessibleName, ControlType, i)) return c15;

                                                                                                                        if (c15.Controls.Count > 0)
                                                                                                                        {
                                                                                                                            //...
                                                                                                                        }
                                                                                                                    }
                                                                                                                    #endregion // Control13
                                                                                                                }
                                                                                                            }
                                                                                                            #endregion // Control14
                                                                                                        }
                                                                                                    }
                                                                                                    #endregion // Control13
                                                                                                }
                                                                                            }
                                                                                            #endregion // Control12
                                                                                        }
                                                                                    }
                                                                                    #endregion // Control11
                                                                                }
                                                                            }
                                                                            #endregion // Control10
                                                                        }
                                                                    }
                                                                    #endregion // Control9
                                                                }
                                                            }
                                                            #endregion // Control8
                                                        }

                                                    }
                                                    #endregion // Control7
                                                }
                                            }
                                            #endregion // Control6
                                        }
                                    }
                                    #endregion // Control5
                                }
                            }
                            #endregion // Control4
                        }

                    }
                    #endregion // Control3
                }
            }
            #endregion // Control2
        }
    }
    */
    #endregion // */


}
