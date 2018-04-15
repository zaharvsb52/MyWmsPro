#region Copyright (c) 2000-2012 Developer Express Inc.
/*
{*******************************************************************}
{                                                                   }
{       Developer Express .NET Component Library                    }
{                                                                   }
{                                                                   }
{       Copyright (c) 2000-2012 Developer Express Inc.              }
{       ALL RIGHTS RESERVED                                         }
{                                                                   }
{   The entire contents of this file is protected by U.S. and       }
{   International Copyright Laws. Unauthorized reproduction,        }
{   reverse-engineering, and distribution of all or any portion of  }
{   the code contained in this file is strictly prohibited and may  }
{   result in severe civil and criminal penalties and will be       }
{   prosecuted to the maximum extent possible under the law.        }
{                                                                   }
{   RESTRICTIONS                                                    }
{                                                                   }
{   THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES           }
{   ARE CONFIDENTIAL AND PROPRIETARY TRADE                          }
{   SECRETS OF DEVELOPER EXPRESS INC. THE REGISTERED DEVELOPER IS   }
{   LICENSED TO DISTRIBUTE THE PRODUCT AND ALL ACCOMPANYING .NET    }
{   CONTROLS AS PART OF AN EXECUTABLE PROGRAM ONLY.                 }
{                                                                   }
{   THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED      }
{   FILES OR ANY PORTION OF ITS CONTENTS SHALL AT NO TIME BE        }
{   COPIED, TRANSFERRED, SOLD, DISTRIBUTED, OR OTHERWISE MADE       }
{   AVAILABLE TO OTHER INDIVIDUALS WITHOUT EXPRESS WRITTEN CONSENT  }
{   AND PERMISSION FROM DEVELOPER EXPRESS INC.                      }
{                                                                   }
{   CONSULT THE END USER LICENSE AGREEMENT FOR INFORMATION ON       }
{   ADDITIONAL RESTRICTIONS.                                        }
{                                                                   }
{*******************************************************************}
*/
#endregion Copyright (c) 2000-2012 Developer Express Inc.

using System.Windows.Markup;
using System.Resources;
#if !SL
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System;
using System.Security;
using DevExpress.Utils;
using DevExpress.Xpf.Core.Native;
#if DEBUGTEST
using NUnit.Framework;
#endif
#else
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DevExpress.Internal;
using System.Runtime.CompilerServices;
#endif
#if !SL
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
#endif
[assembly: AssemblyDescription("Xpf.Core Library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Developer Express Inc.")]
[assembly: AssemblyProduct("DevExpress.Xpf.Themes.GrayOne.Theme")]
[assembly: AssemblyCopyright("Copyright (c) 2000-2012 Developer Express Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
#if !SL
[assembly: AllowPartiallyTrustedCallers]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
#endif
[assembly: SatelliteContractVersion(AssemblyInfo.SatelliteContractVersion)]
[assembly: ComVisible(false)]
[assembly: Guid("ED998E8A-6699-4665-ACB1-7CF9D6E99865")]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]
#pragma warning disable 1699
#if !SL
[assembly: ToolboxTabName(AssemblyInfo.DXTabWpfCommon)]
[assembly: AssemblyKeyFile(@"")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
#endif
#pragma warning restore 1699
