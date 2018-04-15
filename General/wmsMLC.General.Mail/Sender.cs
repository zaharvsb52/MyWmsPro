#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="Sender.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>03.09.2012 13:21:47</Date>
/// <Summary>Отправщик почтовых уведомлений</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace wmsMLC.General.Mail
{
    /// <summary>
    /// Объект отправки почтового уведомления
    /// </summary>
    public class Sender : IDisposable
    {
        /// <summary>
        /// почтовое сообщение
        /// </summary>
        private readonly MailMessage _message;

        /// <summary>
        /// клиент почтового сервера
        /// </summary>
        private readonly SmtpClient _client;        

        /// <summary>
        /// конструктор объекта почтового уведомления
        /// </summary>
        public Sender()
        {
            _message = new MailMessage();
            _client = new SmtpClient();
        }

        /// <summary>
        /// Конструктор с адресом хоста сервера
        /// </summary>
        /// <param name="host">string</param>
        public Sender(object host)
        {
            _message = new MailMessage();
            _client = new SmtpClient(host.ToString());
        }

        /// <summary>
        /// Конструктор с адресом и портом хоста сервера
        /// </summary>
        /// <param name="host">string</param>
        /// <param name="port">int</param>
        public Sender(object host, object port)
        {            
            var myPort = (int)Convert.ChangeType(port, typeof(int));
            _message = new MailMessage();
            _client = new SmtpClient(host.ToString(), myPort);
        }

        /// <summary>
        /// Отправитель письма
        /// </summary>
        /// <param name="address">string</param>
        public void SetFrom(object address)
        {
            _message.From = new MailAddress(address.ToString());            
        }

        /// <summary>
        /// Получатель письма
        /// </summary>
        /// <param name="address">string</param>
        public void AddTo(object address)
        {
            if (address != null)
            {
                var myAddress = address.ToString();
                if (!string.IsNullOrEmpty(myAddress))
                {
                    _message.To.Add(new MailAddress(myAddress));
                }
            }
        }

        /// <summary>
        /// Получатели письма
        /// </summary>
        /// <param name="addresses">string[]</param>
        public void AddTo(object[] addresses)
        {
            foreach (var address in addresses.Where(address => address != null).Where(address => address.ToString() != string.Empty))
            {
                _message.To.Add(new MailAddress(address.ToString()));
            }            
        }

        public void AddBcc(object address)
        {
            if (address != null)
            {
                var myAddress = address.ToString();
                if (!string.IsNullOrEmpty(myAddress))
                {
                    _message.Bcc.Add(new MailAddress(myAddress));
                }
            }
        }

        public void AddBcc(object[] addresses)
        {
            foreach (var address in addresses.Where(address => address != null).Where(address => address.ToString() != string.Empty))
            {
                _message.Bcc.Add(new MailAddress(address.ToString()));
            }   
        }

        /// <summary>
        /// Тема письма
        /// </summary>
        /// <param name="subject">string</param>
        public void SetSubject(object subject)
        {
            _message.Subject = subject.ToString();            
        }

        /// <summary>
        /// Тело письма
        /// </summary>
        /// <param name="body">string</param>
        /// <param name="html">bool</param>
        public void SetBody(object body, object html)
        {            
            _message.IsBodyHtml = (bool)Convert.ChangeType(html, typeof(bool));
            _message.Body = body.ToString();            
        }

        /// <summary>
        /// Добавление к телу письма
        /// </summary>
        /// <param name="body">string</param>        
        public void AppendBody(object body)
        {            
            _message.Body = _message.Body + "\r\n" + body;            
        }

        /// <summary>
        /// Хост почтового сервера
        /// </summary>
        /// <param name="host">string</param>
        public void SetHost(object host)
        { 
            _client.Host = host.ToString();            
        }

        /// <summary>
        /// Хост и порт почтового сервера
        /// </summary>
        /// <param name="host">string</param>
        /// <param name="port">int</param>
        public void SetHost(object host, object port)
        {            
            _client.Host = host.ToString();
            _client.Port = (int)Convert.ChangeType(port, typeof(int));
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="login">string</param>
        /// <param name="password">string</param>
        public void SetCredentials(object login, object password)
        {
            _client.Credentials = new System.Net.NetworkCredential(login.ToString(), password.ToString());            
        }

        /// <summary>
        /// Отправка почтового уведомления </summary>
        public void Send()
        {
            _client.Send(_message);            
        }

        /// <summary>
        /// Отправка почтового уведомления
        /// </summary>
        /// <param name="message">System.Net.Mail.MailMessage</param>
        public void Send(object message)
        {
            var myMessage = (MailMessage)message;
            _client.Send(myMessage);            
        }

        /// <summary>
        /// Отправка почтового уведомления
        /// </summary>
        /// <param name="from">string</param>
        /// <param name="recepients">string ';'</param>
        /// <param name="subject">string</param>
        /// <param name="body">string</param>
        public void Send(object from, object recepients, object subject, object body)
        {
            _client.Send(from.ToString(), recepients.ToString(), subject.ToString(), body.ToString());            
        }

        public void AddAttach(object fileName)
        {
            var attach = new Attachment(fileName.ToString());            
            _message.Attachments.Add(attach);
        }        

        /// <summary>
        /// Добавление вложенного файла
        /// </summary>
        /// <param name="stream">поток данных</param>
        /// <param name="name">имя вложения</param>        
        public void AddAttach(Stream stream, string name)
        {
            Attachment attach = AttachmentHelper.CreateAttachment(stream, name, TransferEncoding.Base64);
            _message.Attachments.Add(attach);
        }

        public void SetBodyFromFile(object fileName, object html)
        {
            var body = File.ReadAllText(fileName.ToString(), Encoding.Default);
            var isHtmlBody = (bool) Convert.ChangeType(html, typeof (bool));
            _message.IsBodyHtml = isHtmlBody;
            _message.Body = body;
            //_message.BodyEncoding = Encoding.Default;            
        }

        public void Dispose()
        {
            if (_message == null)
                return;

            //_message.Attachments.Dispose();
            _message.Dispose();
        }
    }
}
