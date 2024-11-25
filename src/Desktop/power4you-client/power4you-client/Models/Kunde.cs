using System;
using System.ComponentModel;

namespace power4you_client.Models
{
    public class Kunde : INotifyPropertyChanged
    {
        private int _kundenID;
        private string _vorname;
        private string _nachname;
        private string _email;
        private string _telefon;
        private string _plz;
        private string _ort;

        public Kunde()
        {
            _vorname = string.Empty;
            _nachname = string.Empty;
            _email = string.Empty;
            _telefon = string.Empty;
            _plz = string.Empty;
            _ort = string.Empty;
        }

        public int KundenID
        {
            get { return _kundenID; }
            set
            {
                if (_kundenID != value)
                {
                    _kundenID = value;
                    OnPropertyChanged(nameof(KundenID));
                }
            }
        }

        public string Vorname
        {
            get { return _vorname; }
            set
            {
                if (_vorname != value)
                {
                    _vorname = value;
                    OnPropertyChanged(nameof(Vorname));
                }
            }
        }

        public string Nachname
        {
            get { return _nachname; }
            set
            {
                if (_nachname != value)
                {
                    _nachname = value;
                    OnPropertyChanged(nameof(Nachname));
                }
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        public string Telefon
        {
            get { return _telefon; }
            set
            {
                if (_telefon != value)
                {
                    _telefon = value;
                    OnPropertyChanged(nameof(Telefon));
                }
            }
        }

        public string PLZ
        {
            get { return _plz; }
            set
            {
                if (_plz != value)
                {
                    _plz = value;
                    OnPropertyChanged(nameof(PLZ));
                }
            }
        }

        public string Ort
        {
            get { return _ort; }
            set
            {
                if (_ort != value)
                {
                    _ort = value;
                    OnPropertyChanged(nameof(Ort));
                }
            }
        }

        public DateTime Geburtsdatum { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}