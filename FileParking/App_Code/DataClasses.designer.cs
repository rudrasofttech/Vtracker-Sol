﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;



[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="RSTFileParking")]
public partial class FileParkingDataContext : System.Data.Linq.DataContext
{
	
	private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
	
  #region Extensibility Method Definitions
  partial void OnCreated();
  partial void InsertTransfer(Transfer instance);
  partial void UpdateTransfer(Transfer instance);
  partial void DeleteTransfer(Transfer instance);
  partial void InsertParkedFile(ParkedFile instance);
  partial void UpdateParkedFile(ParkedFile instance);
  partial void DeleteParkedFile(ParkedFile instance);
  partial void InsertRecipient(Recipient instance);
  partial void UpdateRecipient(Recipient instance);
  partial void DeleteRecipient(Recipient instance);
  partial void InsertEmailMessage(EmailMessage instance);
  partial void UpdateEmailMessage(EmailMessage instance);
  partial void DeleteEmailMessage(EmailMessage instance);
  partial void InsertMember(Member instance);
  partial void UpdateMember(Member instance);
  partial void DeleteMember(Member instance);
  #endregion
	
	public FileParkingDataContext() : 
			base(global::System.Configuration.ConfigurationManager.ConnectionStrings["RSTFileParkingConnectionString"].ConnectionString, mappingSource)
	{
		OnCreated();
	}
	
	public FileParkingDataContext(string connection) : 
			base(connection, mappingSource)
	{
		OnCreated();
	}
	
	public FileParkingDataContext(System.Data.IDbConnection connection) : 
			base(connection, mappingSource)
	{
		OnCreated();
	}
	
	public FileParkingDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
			base(connection, mappingSource)
	{
		OnCreated();
	}
	
	public FileParkingDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
			base(connection, mappingSource)
	{
		OnCreated();
	}
	
	public System.Data.Linq.Table<Transfer> Transfers
	{
		get
		{
			return this.GetTable<Transfer>();
		}
	}
	
	public System.Data.Linq.Table<ParkedFile> ParkedFiles
	{
		get
		{
			return this.GetTable<ParkedFile>();
		}
	}
	
	public System.Data.Linq.Table<Recipient> Recipients
	{
		get
		{
			return this.GetTable<Recipient>();
		}
	}
	
	public System.Data.Linq.Table<EmailMessage> EmailMessages
	{
		get
		{
			return this.GetTable<EmailMessage>();
		}
	}
	
	public System.Data.Linq.Table<Member> Members
	{
		get
		{
			return this.GetTable<Member>();
		}
	}
}

[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Transfer")]
public partial class Transfer : INotifyPropertyChanging, INotifyPropertyChanged
{
	
	private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
	
	private int _Id;
	
	private string _Message;
	
	private string _Subject;
	
	private int _MemberID;
	
	private int _RecipientID;
	
	private System.DateTime _DateCreated;
	
	private System.Nullable<System.DateTime> _DateRead;
	
	private EntityRef<Recipient> _Recipient;
	
	private EntityRef<Member> _Member;
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(int value);
    partial void OnIdChanged();
    partial void OnMessageChanging(string value);
    partial void OnMessageChanged();
    partial void OnSubjectChanging(string value);
    partial void OnSubjectChanged();
    partial void OnMemberIDChanging(int value);
    partial void OnMemberIDChanged();
    partial void OnRecipientIDChanging(int value);
    partial void OnRecipientIDChanged();
    partial void OnDateCreatedChanging(System.DateTime value);
    partial void OnDateCreatedChanged();
    partial void OnDateReadChanging(System.Nullable<System.DateTime> value);
    partial void OnDateReadChanged();
    #endregion
	
	public Transfer()
	{
		this._Recipient = default(EntityRef<Recipient>);
		this._Member = default(EntityRef<Member>);
		OnCreated();
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
	public int Id
	{
		get
		{
			return this._Id;
		}
		set
		{
			if ((this._Id != value))
			{
				this.OnIdChanging(value);
				this.SendPropertyChanging();
				this._Id = value;
				this.SendPropertyChanged("Id");
				this.OnIdChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Message", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
	public string Message
	{
		get
		{
			return this._Message;
		}
		set
		{
			if ((this._Message != value))
			{
				this.OnMessageChanging(value);
				this.SendPropertyChanging();
				this._Message = value;
				this.SendPropertyChanged("Message");
				this.OnMessageChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Subject", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
	public string Subject
	{
		get
		{
			return this._Subject;
		}
		set
		{
			if ((this._Subject != value))
			{
				this.OnSubjectChanging(value);
				this.SendPropertyChanging();
				this._Subject = value;
				this.SendPropertyChanged("Subject");
				this.OnSubjectChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MemberID", DbType="Int NOT NULL")]
	public int MemberID
	{
		get
		{
			return this._MemberID;
		}
		set
		{
			if ((this._MemberID != value))
			{
				if (this._Member.HasLoadedOrAssignedValue)
				{
					throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
				}
				this.OnMemberIDChanging(value);
				this.SendPropertyChanging();
				this._MemberID = value;
				this.SendPropertyChanged("MemberID");
				this.OnMemberIDChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RecipientID", DbType="Int NOT NULL")]
	public int RecipientID
	{
		get
		{
			return this._RecipientID;
		}
		set
		{
			if ((this._RecipientID != value))
			{
				if (this._Recipient.HasLoadedOrAssignedValue)
				{
					throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
				}
				this.OnRecipientIDChanging(value);
				this.SendPropertyChanging();
				this._RecipientID = value;
				this.SendPropertyChanged("RecipientID");
				this.OnRecipientIDChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DateCreated", DbType="DateTime NOT NULL")]
	public System.DateTime DateCreated
	{
		get
		{
			return this._DateCreated;
		}
		set
		{
			if ((this._DateCreated != value))
			{
				this.OnDateCreatedChanging(value);
				this.SendPropertyChanging();
				this._DateCreated = value;
				this.SendPropertyChanged("DateCreated");
				this.OnDateCreatedChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DateRead", DbType="DateTime")]
	public System.Nullable<System.DateTime> DateRead
	{
		get
		{
			return this._DateRead;
		}
		set
		{
			if ((this._DateRead != value))
			{
				this.OnDateReadChanging(value);
				this.SendPropertyChanging();
				this._DateRead = value;
				this.SendPropertyChanged("DateRead");
				this.OnDateReadChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Recipient_Transfer", Storage="_Recipient", ThisKey="RecipientID", OtherKey="Id", IsForeignKey=true)]
	public Recipient Recipient
	{
		get
		{
			return this._Recipient.Entity;
		}
		set
		{
			Recipient previousValue = this._Recipient.Entity;
			if (((previousValue != value) 
						|| (this._Recipient.HasLoadedOrAssignedValue == false)))
			{
				this.SendPropertyChanging();
				if ((previousValue != null))
				{
					this._Recipient.Entity = null;
					previousValue.Transfers.Remove(this);
				}
				this._Recipient.Entity = value;
				if ((value != null))
				{
					value.Transfers.Add(this);
					this._RecipientID = value.Id;
				}
				else
				{
					this._RecipientID = default(int);
				}
				this.SendPropertyChanged("Recipient");
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Member_Transfer", Storage="_Member", ThisKey="MemberID", OtherKey="Id", IsForeignKey=true)]
	public Member Member
	{
		get
		{
			return this._Member.Entity;
		}
		set
		{
			Member previousValue = this._Member.Entity;
			if (((previousValue != value) 
						|| (this._Member.HasLoadedOrAssignedValue == false)))
			{
				this.SendPropertyChanging();
				if ((previousValue != null))
				{
					this._Member.Entity = null;
					previousValue.Transfers.Remove(this);
				}
				this._Member.Entity = value;
				if ((value != null))
				{
					value.Transfers.Add(this);
					this._MemberID = value.Id;
				}
				else
				{
					this._MemberID = default(int);
				}
				this.SendPropertyChanged("Member");
			}
		}
	}
	
	public event PropertyChangingEventHandler PropertyChanging;
	
	public event PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		if ((this.PropertyChanging != null))
		{
			this.PropertyChanging(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(String propertyName)
	{
		if ((this.PropertyChanged != null))
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ParkedFile")]
public partial class ParkedFile : INotifyPropertyChanging, INotifyPropertyChanged
{
	
	private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
	
	private System.Guid _Id;
	
	private string _FileName;
	
	private System.DateTime _DateCreated;
	
	private System.DateTime _ExpiryDate;
	
	private int _MemberID;
	
	private EntityRef<Member> _Member;
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(System.Guid value);
    partial void OnIdChanged();
    partial void OnFileNameChanging(string value);
    partial void OnFileNameChanged();
    partial void OnDateCreatedChanging(System.DateTime value);
    partial void OnDateCreatedChanged();
    partial void OnExpiryDateChanging(System.DateTime value);
    partial void OnExpiryDateChanged();
    partial void OnMemberIDChanging(int value);
    partial void OnMemberIDChanged();
    #endregion
	
	public ParkedFile()
	{
		this._Member = default(EntityRef<Member>);
		OnCreated();
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
	public System.Guid Id
	{
		get
		{
			return this._Id;
		}
		set
		{
			if ((this._Id != value))
			{
				this.OnIdChanging(value);
				this.SendPropertyChanging();
				this._Id = value;
				this.SendPropertyChanged("Id");
				this.OnIdChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FileName", DbType="NVarChar(1000) NOT NULL", CanBeNull=false)]
	public string FileName
	{
		get
		{
			return this._FileName;
		}
		set
		{
			if ((this._FileName != value))
			{
				this.OnFileNameChanging(value);
				this.SendPropertyChanging();
				this._FileName = value;
				this.SendPropertyChanged("FileName");
				this.OnFileNameChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DateCreated", DbType="DateTime NOT NULL")]
	public System.DateTime DateCreated
	{
		get
		{
			return this._DateCreated;
		}
		set
		{
			if ((this._DateCreated != value))
			{
				this.OnDateCreatedChanging(value);
				this.SendPropertyChanging();
				this._DateCreated = value;
				this.SendPropertyChanged("DateCreated");
				this.OnDateCreatedChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ExpiryDate", DbType="DateTime NOT NULL")]
	public System.DateTime ExpiryDate
	{
		get
		{
			return this._ExpiryDate;
		}
		set
		{
			if ((this._ExpiryDate != value))
			{
				this.OnExpiryDateChanging(value);
				this.SendPropertyChanging();
				this._ExpiryDate = value;
				this.SendPropertyChanged("ExpiryDate");
				this.OnExpiryDateChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MemberID", DbType="Int NOT NULL")]
	public int MemberID
	{
		get
		{
			return this._MemberID;
		}
		set
		{
			if ((this._MemberID != value))
			{
				if (this._Member.HasLoadedOrAssignedValue)
				{
					throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
				}
				this.OnMemberIDChanging(value);
				this.SendPropertyChanging();
				this._MemberID = value;
				this.SendPropertyChanged("MemberID");
				this.OnMemberIDChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Member_ParkedFile", Storage="_Member", ThisKey="MemberID", OtherKey="Id", IsForeignKey=true)]
	public Member Member
	{
		get
		{
			return this._Member.Entity;
		}
		set
		{
			Member previousValue = this._Member.Entity;
			if (((previousValue != value) 
						|| (this._Member.HasLoadedOrAssignedValue == false)))
			{
				this.SendPropertyChanging();
				if ((previousValue != null))
				{
					this._Member.Entity = null;
					previousValue.ParkedFiles.Remove(this);
				}
				this._Member.Entity = value;
				if ((value != null))
				{
					value.ParkedFiles.Add(this);
					this._MemberID = value.Id;
				}
				else
				{
					this._MemberID = default(int);
				}
				this.SendPropertyChanged("Member");
			}
		}
	}
	
	public event PropertyChangingEventHandler PropertyChanging;
	
	public event PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		if ((this.PropertyChanging != null))
		{
			this.PropertyChanging(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(String propertyName)
	{
		if ((this.PropertyChanged != null))
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Recipient")]
public partial class Recipient : INotifyPropertyChanging, INotifyPropertyChanged
{
	
	private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
	
	private int _Id;
	
	private string _Name;
	
	private string _Email;
	
	private int _MemberID;
	
	private EntitySet<Transfer> _Transfers;
	
	private EntityRef<Member> _Member;
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(int value);
    partial void OnIdChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnEmailChanging(string value);
    partial void OnEmailChanged();
    partial void OnMemberIDChanging(int value);
    partial void OnMemberIDChanged();
    #endregion
	
	public Recipient()
	{
		this._Transfers = new EntitySet<Transfer>(new Action<Transfer>(this.attach_Transfers), new Action<Transfer>(this.detach_Transfers));
		this._Member = default(EntityRef<Member>);
		OnCreated();
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
	public int Id
	{
		get
		{
			return this._Id;
		}
		set
		{
			if ((this._Id != value))
			{
				this.OnIdChanging(value);
				this.SendPropertyChanging();
				this._Id = value;
				this.SendPropertyChanged("Id");
				this.OnIdChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
	public string Name
	{
		get
		{
			return this._Name;
		}
		set
		{
			if ((this._Name != value))
			{
				this.OnNameChanging(value);
				this.SendPropertyChanging();
				this._Name = value;
				this.SendPropertyChanged("Name");
				this.OnNameChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Email", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
	public string Email
	{
		get
		{
			return this._Email;
		}
		set
		{
			if ((this._Email != value))
			{
				this.OnEmailChanging(value);
				this.SendPropertyChanging();
				this._Email = value;
				this.SendPropertyChanged("Email");
				this.OnEmailChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MemberID", DbType="Int NOT NULL")]
	public int MemberID
	{
		get
		{
			return this._MemberID;
		}
		set
		{
			if ((this._MemberID != value))
			{
				if (this._Member.HasLoadedOrAssignedValue)
				{
					throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
				}
				this.OnMemberIDChanging(value);
				this.SendPropertyChanging();
				this._MemberID = value;
				this.SendPropertyChanged("MemberID");
				this.OnMemberIDChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Recipient_Transfer", Storage="_Transfers", ThisKey="Id", OtherKey="RecipientID")]
	public EntitySet<Transfer> Transfers
	{
		get
		{
			return this._Transfers;
		}
		set
		{
			this._Transfers.Assign(value);
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Member_Recipient", Storage="_Member", ThisKey="MemberID", OtherKey="Id", IsForeignKey=true)]
	public Member Member
	{
		get
		{
			return this._Member.Entity;
		}
		set
		{
			Member previousValue = this._Member.Entity;
			if (((previousValue != value) 
						|| (this._Member.HasLoadedOrAssignedValue == false)))
			{
				this.SendPropertyChanging();
				if ((previousValue != null))
				{
					this._Member.Entity = null;
					previousValue.Recipients.Remove(this);
				}
				this._Member.Entity = value;
				if ((value != null))
				{
					value.Recipients.Add(this);
					this._MemberID = value.Id;
				}
				else
				{
					this._MemberID = default(int);
				}
				this.SendPropertyChanged("Member");
			}
		}
	}
	
	public event PropertyChangingEventHandler PropertyChanging;
	
	public event PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		if ((this.PropertyChanging != null))
		{
			this.PropertyChanging(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(String propertyName)
	{
		if ((this.PropertyChanged != null))
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	
	private void attach_Transfers(Transfer entity)
	{
		this.SendPropertyChanging();
		entity.Recipient = this;
	}
	
	private void detach_Transfers(Transfer entity)
	{
		this.SendPropertyChanging();
		entity.Recipient = null;
	}
}

[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.EmailMessage")]
public partial class EmailMessage : INotifyPropertyChanging, INotifyPropertyChanged
{
	
	private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
	
	private System.Guid _ID;
	
	private string _FromAddress;
	
	private string _ToAddress;
	
	private string _Subject;
	
	private string _Message;
	
	private bool _IsRead;
	
	private System.DateTime _SentDate;
	
	private System.DateTime _CreateDate;
	
	private bool _IsSent;
	
	private byte _EmailType;
	
	private string _EmailGroup;
	
	private System.Nullable<System.DateTime> _ReadDate;
	
	private string _CCAdress;
	
	private string _ToName;
	
	private string _FromName;
	
	private System.DateTime _LastAttempt;
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(System.Guid value);
    partial void OnIDChanged();
    partial void OnFromAddressChanging(string value);
    partial void OnFromAddressChanged();
    partial void OnToAddressChanging(string value);
    partial void OnToAddressChanged();
    partial void OnSubjectChanging(string value);
    partial void OnSubjectChanged();
    partial void OnMessageChanging(string value);
    partial void OnMessageChanged();
    partial void OnIsReadChanging(bool value);
    partial void OnIsReadChanged();
    partial void OnSentDateChanging(System.DateTime value);
    partial void OnSentDateChanged();
    partial void OnCreateDateChanging(System.DateTime value);
    partial void OnCreateDateChanged();
    partial void OnIsSentChanging(bool value);
    partial void OnIsSentChanged();
    partial void OnEmailTypeChanging(byte value);
    partial void OnEmailTypeChanged();
    partial void OnEmailGroupChanging(string value);
    partial void OnEmailGroupChanged();
    partial void OnReadDateChanging(System.Nullable<System.DateTime> value);
    partial void OnReadDateChanged();
    partial void OnCCAdressChanging(string value);
    partial void OnCCAdressChanged();
    partial void OnToNameChanging(string value);
    partial void OnToNameChanged();
    partial void OnFromNameChanging(string value);
    partial void OnFromNameChanged();
    partial void OnLastAttemptChanging(System.DateTime value);
    partial void OnLastAttemptChanged();
    #endregion
	
	public EmailMessage()
	{
		OnCreated();
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
	public System.Guid ID
	{
		get
		{
			return this._ID;
		}
		set
		{
			if ((this._ID != value))
			{
				this.OnIDChanging(value);
				this.SendPropertyChanging();
				this._ID = value;
				this.SendPropertyChanged("ID");
				this.OnIDChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FromAddress", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
	public string FromAddress
	{
		get
		{
			return this._FromAddress;
		}
		set
		{
			if ((this._FromAddress != value))
			{
				this.OnFromAddressChanging(value);
				this.SendPropertyChanging();
				this._FromAddress = value;
				this.SendPropertyChanged("FromAddress");
				this.OnFromAddressChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ToAddress", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
	public string ToAddress
	{
		get
		{
			return this._ToAddress;
		}
		set
		{
			if ((this._ToAddress != value))
			{
				this.OnToAddressChanging(value);
				this.SendPropertyChanging();
				this._ToAddress = value;
				this.SendPropertyChanged("ToAddress");
				this.OnToAddressChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Subject", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
	public string Subject
	{
		get
		{
			return this._Subject;
		}
		set
		{
			if ((this._Subject != value))
			{
				this.OnSubjectChanging(value);
				this.SendPropertyChanging();
				this._Subject = value;
				this.SendPropertyChanged("Subject");
				this.OnSubjectChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Message", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
	public string Message
	{
		get
		{
			return this._Message;
		}
		set
		{
			if ((this._Message != value))
			{
				this.OnMessageChanging(value);
				this.SendPropertyChanging();
				this._Message = value;
				this.SendPropertyChanged("Message");
				this.OnMessageChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsRead", DbType="Bit NOT NULL")]
	public bool IsRead
	{
		get
		{
			return this._IsRead;
		}
		set
		{
			if ((this._IsRead != value))
			{
				this.OnIsReadChanging(value);
				this.SendPropertyChanging();
				this._IsRead = value;
				this.SendPropertyChanged("IsRead");
				this.OnIsReadChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SentDate", DbType="DateTime NOT NULL")]
	public System.DateTime SentDate
	{
		get
		{
			return this._SentDate;
		}
		set
		{
			if ((this._SentDate != value))
			{
				this.OnSentDateChanging(value);
				this.SendPropertyChanging();
				this._SentDate = value;
				this.SendPropertyChanged("SentDate");
				this.OnSentDateChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreateDate", DbType="DateTime NOT NULL")]
	public System.DateTime CreateDate
	{
		get
		{
			return this._CreateDate;
		}
		set
		{
			if ((this._CreateDate != value))
			{
				this.OnCreateDateChanging(value);
				this.SendPropertyChanging();
				this._CreateDate = value;
				this.SendPropertyChanged("CreateDate");
				this.OnCreateDateChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsSent", DbType="Bit NOT NULL")]
	public bool IsSent
	{
		get
		{
			return this._IsSent;
		}
		set
		{
			if ((this._IsSent != value))
			{
				this.OnIsSentChanging(value);
				this.SendPropertyChanging();
				this._IsSent = value;
				this.SendPropertyChanged("IsSent");
				this.OnIsSentChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EmailType", DbType="TinyInt NOT NULL")]
	public byte EmailType
	{
		get
		{
			return this._EmailType;
		}
		set
		{
			if ((this._EmailType != value))
			{
				this.OnEmailTypeChanging(value);
				this.SendPropertyChanging();
				this._EmailType = value;
				this.SendPropertyChanged("EmailType");
				this.OnEmailTypeChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EmailGroup", DbType="NChar(50) NOT NULL", CanBeNull=false)]
	public string EmailGroup
	{
		get
		{
			return this._EmailGroup;
		}
		set
		{
			if ((this._EmailGroup != value))
			{
				this.OnEmailGroupChanging(value);
				this.SendPropertyChanging();
				this._EmailGroup = value;
				this.SendPropertyChanged("EmailGroup");
				this.OnEmailGroupChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ReadDate", DbType="DateTime")]
	public System.Nullable<System.DateTime> ReadDate
	{
		get
		{
			return this._ReadDate;
		}
		set
		{
			if ((this._ReadDate != value))
			{
				this.OnReadDateChanging(value);
				this.SendPropertyChanging();
				this._ReadDate = value;
				this.SendPropertyChanged("ReadDate");
				this.OnReadDateChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CCAdress", DbType="NVarChar(500) NOT NULL", CanBeNull=false)]
	public string CCAdress
	{
		get
		{
			return this._CCAdress;
		}
		set
		{
			if ((this._CCAdress != value))
			{
				this.OnCCAdressChanging(value);
				this.SendPropertyChanging();
				this._CCAdress = value;
				this.SendPropertyChanged("CCAdress");
				this.OnCCAdressChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ToName", DbType="NVarChar(150) NOT NULL", CanBeNull=false)]
	public string ToName
	{
		get
		{
			return this._ToName;
		}
		set
		{
			if ((this._ToName != value))
			{
				this.OnToNameChanging(value);
				this.SendPropertyChanging();
				this._ToName = value;
				this.SendPropertyChanged("ToName");
				this.OnToNameChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FromName", DbType="NVarChar(150) NOT NULL", CanBeNull=false)]
	public string FromName
	{
		get
		{
			return this._FromName;
		}
		set
		{
			if ((this._FromName != value))
			{
				this.OnFromNameChanging(value);
				this.SendPropertyChanging();
				this._FromName = value;
				this.SendPropertyChanged("FromName");
				this.OnFromNameChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastAttempt", DbType="DateTime NOT NULL")]
	public System.DateTime LastAttempt
	{
		get
		{
			return this._LastAttempt;
		}
		set
		{
			if ((this._LastAttempt != value))
			{
				this.OnLastAttemptChanging(value);
				this.SendPropertyChanging();
				this._LastAttempt = value;
				this.SendPropertyChanged("LastAttempt");
				this.OnLastAttemptChanged();
			}
		}
	}
	
	public event PropertyChangingEventHandler PropertyChanging;
	
	public event PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		if ((this.PropertyChanging != null))
		{
			this.PropertyChanging(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(String propertyName)
	{
		if ((this.PropertyChanged != null))
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Member")]
public partial class Member : INotifyPropertyChanging, INotifyPropertyChanged
{
	
	private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
	
	private int _Id;
	
	private string _Email;
	
	private string _Password;
	
	private string _FirstName;
	
	private string _LastName;
	
	private string _Folder;
	
	private System.DateTime _DateCreated;
	
	private System.Nullable<System.DateTime> _DateModified;
	
	private byte _Status;
	
	private System.Guid _PublicID;
	
	private System.Nullable<System.Guid> _AuthToken;
	
	private System.Nullable<System.DateTime> _TokenCreated;
	
	private EntitySet<Transfer> _Transfers;
	
	private EntitySet<ParkedFile> _ParkedFiles;
	
	private EntitySet<Recipient> _Recipients;
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(int value);
    partial void OnIdChanged();
    partial void OnEmailChanging(string value);
    partial void OnEmailChanged();
    partial void OnPasswordChanging(string value);
    partial void OnPasswordChanged();
    partial void OnFirstNameChanging(string value);
    partial void OnFirstNameChanged();
    partial void OnLastNameChanging(string value);
    partial void OnLastNameChanged();
    partial void OnFolderChanging(string value);
    partial void OnFolderChanged();
    partial void OnDateCreatedChanging(System.DateTime value);
    partial void OnDateCreatedChanged();
    partial void OnDateModifiedChanging(System.Nullable<System.DateTime> value);
    partial void OnDateModifiedChanged();
    partial void OnStatusChanging(byte value);
    partial void OnStatusChanged();
    partial void OnPublicIDChanging(System.Guid value);
    partial void OnPublicIDChanged();
    partial void OnAuthTokenChanging(System.Nullable<System.Guid> value);
    partial void OnAuthTokenChanged();
    partial void OnTokenCreatedChanging(System.Nullable<System.DateTime> value);
    partial void OnTokenCreatedChanged();
    #endregion
	
	public Member()
	{
		this._Transfers = new EntitySet<Transfer>(new Action<Transfer>(this.attach_Transfers), new Action<Transfer>(this.detach_Transfers));
		this._ParkedFiles = new EntitySet<ParkedFile>(new Action<ParkedFile>(this.attach_ParkedFiles), new Action<ParkedFile>(this.detach_ParkedFiles));
		this._Recipients = new EntitySet<Recipient>(new Action<Recipient>(this.attach_Recipients), new Action<Recipient>(this.detach_Recipients));
		OnCreated();
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
	public int Id
	{
		get
		{
			return this._Id;
		}
		set
		{
			if ((this._Id != value))
			{
				this.OnIdChanging(value);
				this.SendPropertyChanging();
				this._Id = value;
				this.SendPropertyChanged("Id");
				this.OnIdChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Email", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
	public string Email
	{
		get
		{
			return this._Email;
		}
		set
		{
			if ((this._Email != value))
			{
				this.OnEmailChanging(value);
				this.SendPropertyChanging();
				this._Email = value;
				this.SendPropertyChanged("Email");
				this.OnEmailChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Password", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
	public string Password
	{
		get
		{
			return this._Password;
		}
		set
		{
			if ((this._Password != value))
			{
				this.OnPasswordChanging(value);
				this.SendPropertyChanging();
				this._Password = value;
				this.SendPropertyChanged("Password");
				this.OnPasswordChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FirstName", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
	public string FirstName
	{
		get
		{
			return this._FirstName;
		}
		set
		{
			if ((this._FirstName != value))
			{
				this.OnFirstNameChanging(value);
				this.SendPropertyChanging();
				this._FirstName = value;
				this.SendPropertyChanged("FirstName");
				this.OnFirstNameChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastName", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
	public string LastName
	{
		get
		{
			return this._LastName;
		}
		set
		{
			if ((this._LastName != value))
			{
				this.OnLastNameChanging(value);
				this.SendPropertyChanging();
				this._LastName = value;
				this.SendPropertyChanged("LastName");
				this.OnLastNameChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Folder", DbType="NVarChar(500) NOT NULL", CanBeNull=false)]
	public string Folder
	{
		get
		{
			return this._Folder;
		}
		set
		{
			if ((this._Folder != value))
			{
				this.OnFolderChanging(value);
				this.SendPropertyChanging();
				this._Folder = value;
				this.SendPropertyChanged("Folder");
				this.OnFolderChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DateCreated", DbType="DateTime NOT NULL")]
	public System.DateTime DateCreated
	{
		get
		{
			return this._DateCreated;
		}
		set
		{
			if ((this._DateCreated != value))
			{
				this.OnDateCreatedChanging(value);
				this.SendPropertyChanging();
				this._DateCreated = value;
				this.SendPropertyChanged("DateCreated");
				this.OnDateCreatedChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DateModified", DbType="DateTime")]
	public System.Nullable<System.DateTime> DateModified
	{
		get
		{
			return this._DateModified;
		}
		set
		{
			if ((this._DateModified != value))
			{
				this.OnDateModifiedChanging(value);
				this.SendPropertyChanging();
				this._DateModified = value;
				this.SendPropertyChanged("DateModified");
				this.OnDateModifiedChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Status", DbType="TinyInt NOT NULL")]
	public byte Status
	{
		get
		{
			return this._Status;
		}
		set
		{
			if ((this._Status != value))
			{
				this.OnStatusChanging(value);
				this.SendPropertyChanging();
				this._Status = value;
				this.SendPropertyChanged("Status");
				this.OnStatusChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PublicID", DbType="UniqueIdentifier NOT NULL")]
	public System.Guid PublicID
	{
		get
		{
			return this._PublicID;
		}
		set
		{
			if ((this._PublicID != value))
			{
				this.OnPublicIDChanging(value);
				this.SendPropertyChanging();
				this._PublicID = value;
				this.SendPropertyChanged("PublicID");
				this.OnPublicIDChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AuthToken", DbType="UniqueIdentifier")]
	public System.Nullable<System.Guid> AuthToken
	{
		get
		{
			return this._AuthToken;
		}
		set
		{
			if ((this._AuthToken != value))
			{
				this.OnAuthTokenChanging(value);
				this.SendPropertyChanging();
				this._AuthToken = value;
				this.SendPropertyChanged("AuthToken");
				this.OnAuthTokenChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TokenCreated", DbType="DateTime")]
	public System.Nullable<System.DateTime> TokenCreated
	{
		get
		{
			return this._TokenCreated;
		}
		set
		{
			if ((this._TokenCreated != value))
			{
				this.OnTokenCreatedChanging(value);
				this.SendPropertyChanging();
				this._TokenCreated = value;
				this.SendPropertyChanged("TokenCreated");
				this.OnTokenCreatedChanged();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Member_Transfer", Storage="_Transfers", ThisKey="Id", OtherKey="MemberID")]
	public EntitySet<Transfer> Transfers
	{
		get
		{
			return this._Transfers;
		}
		set
		{
			this._Transfers.Assign(value);
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Member_ParkedFile", Storage="_ParkedFiles", ThisKey="Id", OtherKey="MemberID")]
	public EntitySet<ParkedFile> ParkedFiles
	{
		get
		{
			return this._ParkedFiles;
		}
		set
		{
			this._ParkedFiles.Assign(value);
		}
	}
	
	[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Member_Recipient", Storage="_Recipients", ThisKey="Id", OtherKey="MemberID")]
	public EntitySet<Recipient> Recipients
	{
		get
		{
			return this._Recipients;
		}
		set
		{
			this._Recipients.Assign(value);
		}
	}
	
	public event PropertyChangingEventHandler PropertyChanging;
	
	public event PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		if ((this.PropertyChanging != null))
		{
			this.PropertyChanging(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(String propertyName)
	{
		if ((this.PropertyChanged != null))
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	
	private void attach_Transfers(Transfer entity)
	{
		this.SendPropertyChanging();
		entity.Member = this;
	}
	
	private void detach_Transfers(Transfer entity)
	{
		this.SendPropertyChanging();
		entity.Member = null;
	}
	
	private void attach_ParkedFiles(ParkedFile entity)
	{
		this.SendPropertyChanging();
		entity.Member = this;
	}
	
	private void detach_ParkedFiles(ParkedFile entity)
	{
		this.SendPropertyChanging();
		entity.Member = null;
	}
	
	private void attach_Recipients(Recipient entity)
	{
		this.SendPropertyChanging();
		entity.Member = this;
	}
	
	private void detach_Recipients(Recipient entity)
	{
		this.SendPropertyChanging();
		entity.Member = null;
	}
}
#pragma warning restore 1591
