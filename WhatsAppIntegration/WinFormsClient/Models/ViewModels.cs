using System;
using System.ComponentModel;

namespace UstadDesktop.WhatsAppIntegration.WinFormsClient.Models
{
    /// <summary>
    /// View model for conversation display in the WinForms client
    /// </summary>
    public class ConversationViewModel : INotifyPropertyChanged
    {
        private int _id;
        private string _customerNumber;
        private string _customerName;
        private string _lastMessage;
        private DateTime _lastMessageTime;
        private string _status;
        private string _priority;
        private int? _assignedOperatorId;
        private string _assignedOperatorName;
        private int _unreadCount;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string CustomerNumber
        {
            get => _customerNumber;
            set
            {
                if (_customerNumber != value)
                {
                    _customerNumber = value;
                    OnPropertyChanged(nameof(CustomerNumber));
                }
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (_customerName != value)
                {
                    _customerName = value;
                    OnPropertyChanged(nameof(CustomerName));
                }
            }
        }

        public string LastMessage
        {
            get => _lastMessage;
            set
            {
                if (_lastMessage != value)
                {
                    _lastMessage = value;
                    OnPropertyChanged(nameof(LastMessage));
                }
            }
        }

        public DateTime LastMessageTime
        {
            get => _lastMessageTime;
            set
            {
                if (_lastMessageTime != value)
                {
                    _lastMessageTime = value;
                    OnPropertyChanged(nameof(LastMessageTime));
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        public string Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                    OnPropertyChanged(nameof(PriorityDisplay));
                }
            }
        }

        public int? AssignedOperatorId
        {
            get => _assignedOperatorId;
            set
            {
                if (_assignedOperatorId != value)
                {
                    _assignedOperatorId = value;
                    OnPropertyChanged(nameof(AssignedOperatorId));
                }
            }
        }

        public string AssignedOperatorName
        {
            get => _assignedOperatorName;
            set
            {
                if (_assignedOperatorName != value)
                {
                    _assignedOperatorName = value;
                    OnPropertyChanged(nameof(AssignedOperatorName));
                    OnPropertyChanged(nameof(AssignmentDisplay));
                }
            }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                if (_unreadCount != value)
                {
                    _unreadCount = value;
                    OnPropertyChanged(nameof(UnreadCount));
                    OnPropertyChanged(nameof(UnreadDisplay));
                }
            }
        }

        // Display properties for UI
        public string StatusDisplay => Status switch
        {
            "Open" => "Açık",
            "Closed" => "Kapalı",
            "Pending" => "Beklemede",
            _ => Status ?? "Bilinmiyor"
        };

        public string PriorityDisplay => Priority switch
        {
            "Low" => "Düşük",
            "Normal" => "Normal",
            "High" => "Yüksek",
            "Urgent" => "Acil",
            _ => Priority ?? "Normal"
        };

        public string AssignmentDisplay => string.IsNullOrEmpty(AssignedOperatorName) ? "Atanmamış" : AssignedOperatorName;

        public string UnreadDisplay => UnreadCount > 0 ? UnreadCount.ToString() : "";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// View model for message display in the WinForms client
    /// </summary>
    public class MessageViewModel : INotifyPropertyChanged
    {
        private long _id;
        private int _conversationId;
        private string _whatsAppMessageId;
        private string _direction;
        private string _messageType;
        private string _body;
        private string _mediaUrl;
        private int? _operatorId;
        private string _operatorName;
        private bool _isRead;
        private DateTime _createdAt;

        public long Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public int ConversationId
        {
            get => _conversationId;
            set
            {
                if (_conversationId != value)
                {
                    _conversationId = value;
                    OnPropertyChanged(nameof(ConversationId));
                }
            }
        }

        public string WhatsAppMessageId
        {
            get => _whatsAppMessageId;
            set
            {
                if (_whatsAppMessageId != value)
                {
                    _whatsAppMessageId = value;
                    OnPropertyChanged(nameof(WhatsAppMessageId));
                }
            }
        }

        public string Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    OnPropertyChanged(nameof(Direction));
                    OnPropertyChanged(nameof(DirectionDisplay));
                    OnPropertyChanged(nameof(IsIncoming));
                    OnPropertyChanged(nameof(IsOutgoing));
                }
            }
        }

        public string MessageType
        {
            get => _messageType;
            set
            {
                if (_messageType != value)
                {
                    _messageType = value;
                    OnPropertyChanged(nameof(MessageType));
                    OnPropertyChanged(nameof(MessageTypeDisplay));
                }
            }
        }

        public string Body
        {
            get => _body;
            set
            {
                if (_body != value)
                {
                    _body = value;
                    OnPropertyChanged(nameof(Body));
                }
            }
        }

        public string MediaUrl
        {
            get => _mediaUrl;
            set
            {
                if (_mediaUrl != value)
                {
                    _mediaUrl = value;
                    OnPropertyChanged(nameof(MediaUrl));
                    OnPropertyChanged(nameof(HasMedia));
                }
            }
        }

        public int? OperatorId
        {
            get => _operatorId;
            set
            {
                if (_operatorId != value)
                {
                    _operatorId = value;
                    OnPropertyChanged(nameof(OperatorId));
                }
            }
        }

        public string OperatorName
        {
            get => _operatorName;
            set
            {
                if (_operatorName != value)
                {
                    _operatorName = value;
                    OnPropertyChanged(nameof(OperatorName));
                }
            }
        }

        public bool IsRead
        {
            get => _isRead;
            set
            {
                if (_isRead != value)
                {
                    _isRead = value;
                    OnPropertyChanged(nameof(IsRead));
                    OnPropertyChanged(nameof(ReadStatusDisplay));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged(nameof(CreatedAt));
                    OnPropertyChanged(nameof(TimeDisplay));
                }
            }
        }

        // Display properties for UI
        public string DirectionDisplay => Direction == "In" ? "Gelen" : "Giden";
        public bool IsIncoming => Direction == "In";
        public bool IsOutgoing => Direction == "Out";
        public bool HasMedia => !string.IsNullOrEmpty(MediaUrl);

        public string MessageTypeDisplay => MessageType switch
        {
            "text" => "Metin",
            "image" => "Resim",
            "document" => "Belge",
            "audio" => "Ses",
            "video" => "Video",
            "template" => "Şablon",
            _ => MessageType ?? "Bilinmiyor"
        };

        public string ReadStatusDisplay => IsRead ? "Okundu" : "Okunmadı";
        public string TimeDisplay => CreatedAt.ToString("HH:mm");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// View model for operator display in admin panels
    /// </summary>
    public class OperatorViewModel : INotifyPropertyChanged
    {
        private int _id;
        private int _firmId;
        private string _userName;
        private string _fullName;
        private string _email;
        private string _phone;
        private string _role;
        private bool _isActive;
        private int _failedCount;
        private DateTime? _lockoutEnd;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public int FirmId
        {
            get => _firmId;
            set
            {
                if (_firmId != value)
                {
                    _firmId = value;
                    OnPropertyChanged(nameof(FirmId));
                }
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged(nameof(Role));
                    OnPropertyChanged(nameof(RoleDisplay));
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        public int FailedCount
        {
            get => _failedCount;
            set
            {
                if (_failedCount != value)
                {
                    _failedCount = value;
                    OnPropertyChanged(nameof(FailedCount));
                }
            }
        }

        public DateTime? LockoutEnd
        {
            get => _lockoutEnd;
            set
            {
                if (_lockoutEnd != value)
                {
                    _lockoutEnd = value;
                    OnPropertyChanged(nameof(LockoutEnd));
                    OnPropertyChanged(nameof(IsLocked));
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged(nameof(CreatedAt));
                }
            }
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set
            {
                if (_updatedAt != value)
                {
                    _updatedAt = value;
                    OnPropertyChanged(nameof(UpdatedAt));
                }
            }
        }

        // Display properties for UI
        public string RoleDisplay => Role switch
        {
            "Admin" => "Yönetici",
            "Agent" => "Operatör",
            _ => Role ?? "Bilinmiyor"
        };

        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

        public string StatusDisplay
        {
            get
            {
                if (!IsActive) return "Pasif";
                if (IsLocked) return "Kilitli";
                return "Aktif";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// View model for audit log display
    /// </summary>
    public class AuditLogViewModel
    {
        public long Id { get; set; }
        public int FirmId { get; set; }
        public int? OperatorId { get; set; }
        public string OperatorName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display properties
        public string ActionDisplay => Action switch
        {
            "LOGIN_SUCCESS" => "Başarılı Giriş",
            "LOGIN_FAILED" => "Başarısız Giriş",
            "MESSAGE_SENT" => "Mesaj Gönderildi",
            "CONVERSATION_ASSIGNED" => "Konuşma Atandı",
            "CONVERSATION_STATUS_CHANGED" => "Durum Değişti",
            "OPERATOR_CONNECTED" => "Operatör Bağlandı",
            "OPERATOR_DISCONNECTED" => "Operatör Ayrıldı",
            "PASSWORD_RESET_REQUESTED" => "Şifre Sıfırlama Talep Edildi",
            "PASSWORD_RESET_SUCCESS" => "Şifre Sıfırlandı",
            _ => Action ?? "Bilinmiyor"
        };

        public string TimeDisplay => CreatedAt.ToString("dd.MM.yyyy HH:mm:ss");
    }

    /// <summary>
    /// View model for WhatsApp template management
    /// </summary>
    public class TemplateViewModel : INotifyPropertyChanged
    {
        private int _id;
        private int _firmId;
        private string _templateName;
        private string _language;
        private string _category;
        private string _headerText;
        private string _bodyText;
        private string _footerText;
        private string _buttonText;
        private bool _isActive;
        private DateTime _createdAt;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public int FirmId
        {
            get => _firmId;
            set
            {
                if (_firmId != value)
                {
                    _firmId = value;
                    OnPropertyChanged(nameof(FirmId));
                }
            }
        }

        public string TemplateName
        {
            get => _templateName;
            set
            {
                if (_templateName != value)
                {
                    _templateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                }
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged(nameof(Language));
                    OnPropertyChanged(nameof(LanguageDisplay));
                }
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged(nameof(Category));
                    OnPropertyChanged(nameof(CategoryDisplay));
                }
            }
        }

        public string HeaderText
        {
            get => _headerText;
            set
            {
                if (_headerText != value)
                {
                    _headerText = value;
                    OnPropertyChanged(nameof(HeaderText));
                }
            }
        }

        public string BodyText
        {
            get => _bodyText;
            set
            {
                if (_bodyText != value)
                {
                    _bodyText = value;
                    OnPropertyChanged(nameof(BodyText));
                }
            }
        }

        public string FooterText
        {
            get => _footerText;
            set
            {
                if (_footerText != value)
                {
                    _footerText = value;
                    OnPropertyChanged(nameof(FooterText));
                }
            }
        }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                if (_buttonText != value)
                {
                    _buttonText = value;
                    OnPropertyChanged(nameof(ButtonText));
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged(nameof(CreatedAt));
                }
            }
        }

        // Display properties
        public string LanguageDisplay => Language switch
        {
            "tr" => "Türkçe",
            "en" => "İngilizce",
            "ar" => "Arapça",
            _ => Language ?? "Bilinmiyor"
        };

        public string CategoryDisplay => Category switch
        {
            "MARKETING" => "Pazarlama",
            "UTILITY" => "Hizmet",
            "AUTHENTICATION" => "Kimlik Doğrulama",
            _ => Category ?? "Bilinmiyor"
        };

        public string StatusDisplay => IsActive ? "Aktif" : "Pasif";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
