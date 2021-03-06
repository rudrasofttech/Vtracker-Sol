﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileParking;
using System.IO;

namespace FileParking.Models
{
    public class DriveManager
    {
        public Member CurrentMember;
        private string drivePath = string.Empty;
        private string webPath = string.Empty;
        public bool ItemDeletable { get; set; }
        public string MemberDataAbsPath
        {
            get
            {
                return drivePath;
            }
        }

        public string MemberWebPath
        {
            get
            {
                return webPath;
            }
        }

        public DriveManager(Member m, string dataFolderAbsolutePath, string dataFolderWebPath)
        {
            CurrentMember = m;
            drivePath = dataFolderAbsolutePath;
            webPath = dataFolderWebPath;
        }

        public bool DriveExist
        {
            get
            {
                return Directory.Exists(MemberDataAbsPath);
            }
        }


        public bool RenameFile(string filepath, string name)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            string drivepath = Path.Combine(MemberDataAbsPath, filepath);

            FileInfo fi = new FileInfo(drivepath);

            if (fi.Exists)
            {
                string newpath = Path.Combine(fi.DirectoryName, name);
                File.Move(drivepath, newpath);
                return true;
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        public bool FileExists(string filePath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            string drivepath = Path.Combine(MemberDataAbsPath, filePath);

            FileInfo fi = new FileInfo(drivepath);
            return fi.Exists;
        }

        public bool RenameFolder(string folderpath, string name)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            string drivepath = Path.Combine(MemberDataAbsPath, folderpath);

            DirectoryInfo di = new DirectoryInfo(drivepath);

            if (di.Exists)
            {
                string newpath = Path.Combine(di.Parent.FullName, name);
                Directory.Move(drivepath, newpath);

                return true;
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        public RDirectoryItem GetFolderName(string folderPath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            if (folderPath.Trim() != string.Empty)
            {
                string drivepath = Path.Combine(MemberDataAbsPath, folderPath);
                DirectoryInfo i = new DirectoryInfo(drivepath);
                RDirectoryItem rdi = new RDirectoryItem();
                rdi.ID = Guid.NewGuid();
                rdi.CreateDate = i.CreationTime;
                rdi.LastAccessDate = i.LastAccessTime;

                rdi.Location = i.FullName.Replace(string.Format("{0}\\", MemberDataAbsPath), string.Empty);
                rdi.ModifyDate = i.LastWriteTime;
                rdi.Name = i.Name;

                rdi.Contains = ""; // string.Format("{0} Folders, {1} Files", i.EnumerateDirectories().Count(), i.EnumerateFileSystemInfos().Count());

                rdi.ThumbNail = string.Format("{0}/bootstrap/img/drive/folder-data-icon.png", Utility.SiteURL);

                return rdi;
            }
            else
            {
                return new RDirectoryItem();
            }
        }

        public void RemoveExpiredFiles(string folderpath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            List<RFileItem> list = new List<RFileItem>();
            string drivepath = Path.Combine(MemberDataAbsPath, folderpath);
            MemberPlan mp = PlanManager.GetUserActivePlan(CurrentMember.Id);

            DirectoryInfo di = new DirectoryInfo(drivepath);
            if (di.Exists)
            {
                foreach (FileInfo i in di.EnumerateFiles())
                {
                    if((mp.Amount == 0 && i.CreationTimeUtc.AddDays(mp.Term) < DateTime.UtcNow) ||
                        (mp.Amount > 0 && mp.ExpiryDate < DateTime.UtcNow))
                    {
                        i.Delete();
                    }
                }
            }
        }

        public List<RFileItem> GetFileItemList(string folderpath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            List<RFileItem> list = new List<RFileItem>();
            string drivepath = Path.Combine(MemberDataAbsPath, folderpath);
            MemberPlan mp = PlanManager.GetUserActivePlan(CurrentMember.Id);

                DirectoryInfo di = new DirectoryInfo(drivepath);
            if (di.Exists)
            {
                foreach (FileInfo i in di.EnumerateFiles())
                {
                    RFileItem rdi = new RFileItem();
                    rdi.ID = Guid.NewGuid();
                    rdi.ExpiryDate = mp.Amount == 0 ? i.CreationTimeUtc.AddDays(mp.Term) : mp.ExpiryDate;

                    rdi.CreateDate = i.CreationTime;
                    rdi.FileType = i.Extension;
                    rdi.LastAccessDate = i.LastAccessTime;

                    rdi.Location = ""; // i.FullName.Replace(string.Format("{0}\\", MemberDataAbsPath), string.Empty);
                    rdi.ModifyDate = i.LastWriteTime;
                    rdi.Name = i.Name;
                    rdi.Deletable = ItemDeletable;
                    rdi.Editable = false;
                    long length = i.Length;
                    if (length < 1024)
                    {
                        rdi.Size = string.Format("{0} B", i.Length.ToString());
                    }
                    else if (length >= 1024 && length < (1024 * 1024))
                    {
                        rdi.Size = string.Format("{0} KB", (i.Length / 1024).ToString());
                    }
                    else if (length >= (1024 * 1024) && length < (1024 * 1024 * 1024))
                    {
                        rdi.Size = string.Format("{0} MB", ((i.Length / 1024) / 1024).ToString());
                    }

                    rdi.WebPath = string.Format("{0}/{1}", MemberWebPath, rdi.Location.Replace("\\", "/"));
                    if (Utility.ImageFormat().ToLower().IndexOf(rdi.FileType.ToLower()) > -1)
                    {
                        rdi.ItemType = DriveItemType.ImageFile;
                        rdi.ThumbNail = rdi.WebPath;
                    }
                    else if (Utility.VideoFormat().ToLower().IndexOf(rdi.FileType.ToLower()) > -1)
                    {
                        rdi.ItemType = DriveItemType.VideoFile;
                    }
                    else if (Utility.TextFormat().ToLower().IndexOf(rdi.FileType.ToLower()) > -1)
                    {
                        rdi.ItemType = DriveItemType.TextFile;
                    }
                    else if (Utility.CompresssedFormat().ToLower().IndexOf(rdi.FileType.ToLower()) > -1)
                    {
                        rdi.ItemType = DriveItemType.ZipFile;
                    }
                    else
                    {
                        rdi.ItemType = DriveItemType.File;
                    }

                    list.Add(rdi);
                }
            }


            return list;
        }

        public string GetFileSize(string filepath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }
            string absfpath = Path.Combine(MemberDataAbsPath, filepath);
            FileInfo fi = new FileInfo(absfpath);
            string size = "";
            if (fi.Exists)
            {
                long length = fi.Length;
                if (length < 1024)
                {
                    size = string.Format("{0} B", fi.Length.ToString());
                }
                else if (length >= 1024 && length < (1024 * 1024))
                {
                    size = string.Format("{0} KB", (fi.Length / 1024).ToString());
                }
                else if (length >= (1024 * 1024) && length < (1024 * 1024 * 1024))
                {
                    size = string.Format("{0} MB", ((fi.Length / 1024) / 1024).ToString());
                }
            }
            return size;
            
        }

        public List<RDirectoryItem> GetDirectoryItemList(string folderpath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }
            bool folderDeletable = true;
            List<RDirectoryItem> list = new List<RDirectoryItem>();
            string drivepath = Path.Combine(MemberDataAbsPath, folderpath);
            folderDeletable = ItemDeletable;

            if (folderpath == string.Empty)
            {
                folderDeletable = false;
            }

            DirectoryInfo di = new DirectoryInfo(drivepath);
            if (di.Exists)
            {
                foreach (DirectoryInfo i in di.EnumerateDirectories())
                {
                    RDirectoryItem rdi = new RDirectoryItem();
                    rdi.ID = Guid.NewGuid();
                    rdi.CreateDate = i.CreationTime;
                    rdi.LastAccessDate = i.LastAccessTime;
                    rdi.Location = i.FullName.Replace(string.Format("{0}\\", MemberDataAbsPath), string.Empty);
                    rdi.ModifyDate = i.LastWriteTime;
                    rdi.Name = i.Name;
                    rdi.Deletable = folderDeletable;
                    rdi.Editable = true;
                    rdi.Contains = "";// string.Format("{0} Folders, {1} Files", i.EnumerateDirectories().Count(), i.EnumerateFiles().Count());
                    //if (i.GetDirectories().Length > 0 || i.GetFiles().Length > 0)
                    //{
                    rdi.ThumbNail = string.Format("{0}/bootstrap/img/drive/folder-data-icon.png", Utility.SiteURL);
                    //}
                    //else
                    //{
                    //    rdi.ThumbNail = string.Format("{0}/bootstrap/img/drive/folder-empty-icon.png", Utility.SiteURL);
                    //}
                    list.Add(rdi);
                }
            }
            else
            {
                throw new DriveDoesNotExistException();
            }

            return list;
        }

        public bool CreateDriveFolder(string dirPath, DriveItemType itype)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }

            DirectoryInfo fi = new DirectoryInfo(string.Format("{0}\\{1}", MemberDataAbsPath, dirPath));
            fi.Create();
            return fi.Exists;
        }

        public bool DeleteFile(string folderPath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }
            string filepath = Path.Combine(MemberDataAbsPath, folderPath);
            File.Delete(filepath);
            return true;
        }

        public bool DeleteFolder(string folderPath)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }
            string drivepath = Path.Combine(MemberDataAbsPath, folderPath);
            Directory.Delete(drivepath, true);
            return true;
        }

        public void UploadFile(string folderPath, HttpPostedFile pfile)
        {
            if (!DriveExist)
            {
                throw new DriveDoesNotExistException();
            }
            string fname = Path.GetFileName(pfile.FileName);
            string tempfilepath = Path.Combine(MemberDataAbsPath, folderPath, "temp", fname);
            string filepath = Path.Combine(MemberDataAbsPath, folderPath, fname);

            FileInfo fi = new FileInfo(filepath);

            if (!fi.Exists)
            {
                pfile.SaveAs(filepath);
            }
            else
            {
                pfile.SaveAs(tempfilepath);
            }
        }

        public int RemainingFileLimit()
        {
            MemberPlan mp = PlanManager.GetUserActivePlan(CurrentMember.Id);
            int fileCount = GetFileItemList("").Count;
            return mp.Limit - fileCount;
        } 

    }

    public class RDirectoryItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Contains { get; set; }
        public string Size { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public DateTime LastAccessDate { get; set; }
        public bool Deletable { get; set; }
        public bool Editable { get; set; }
        public string ThumbNail { get; set; }
    }

    public class RFileItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public string FileType { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateDateDisplay
        {
            get
            {
                if (CreateDate != null)
                {
                    return CreateDate.ToString("yy/M/d h:mm tt");
                }
                else
                {
                    return "";
                }
            }
        }
        public DateTime ModifyDate { get; set; }
        public string ModifyDateDisplay
        {
            get
            {
                if (ModifyDate != null)
                {
                    return ModifyDate.ToString("yy/M/d h:mm tt");
                }
                else { return ""; }
            }
        }
        public DateTime LastAccessDate { get; set; }
        public bool Deletable { get; set; }
        public bool Editable { get; set; }
        public string ThumbNail { get; set; }
        public DriveItemType ItemType { get; set; }
        public string WebPath { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ExpiryDateDisplay { get {
                if (ExpiryDate != null)
                {
                    return ExpiryDate.ToString("yy/M/d h:mm tt");
                }
                else { return ""; }
            } }
    }

    public enum DriveItemType
    {
        Folder,
        File,
        TextFile,
        ImageFile,
        VideoFile,
        ZipFile
    }

    public class DriveItemDoesNotExistException : Exception { }
    public class DriveDoesNotExistException : Exception { }
    public class DuplicateDriveException : Exception { }
    public class InvalidDriveIdException : Exception { }
}