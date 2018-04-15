﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18449
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace wmsMLC.General.DAL.Oracle {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ExceptionResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("wmsMLC.General.DAL.Oracle.ExceptionResources", typeof(ExceptionResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ошибка работы с данными..
        /// </summary>
        internal static string CommonDataError {
            get {
                return ResourceManager.GetString("CommonDataError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Такой объект уже существует..
        /// </summary>
        internal static string DuplicateKeyError {
            get {
                return ResourceManager.GetString("DuplicateKeyError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Не заполнено обязательное поле &apos;{0}&apos;..
        /// </summary>
        internal static string FieldIsNullError {
            get {
                return ResourceManager.GetString("FieldIsNullError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Техническое ограничение: фильтр не может быть длиннее 4000 символов! Попробуйте упростить условие.
        /// </summary>
        internal static string FilterLengthMoreThan4000 {
            get {
                return ResourceManager.GetString("FilterLengthMoreThan4000", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Отсутствует родительский объект..
        /// </summary>
        internal static string ForeignKeyConstraintError {
            get {
                return ResourceManager.GetString("ForeignKeyConstraintError", resourceCulture);
            }
        }
    }
}
