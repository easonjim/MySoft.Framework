#region Copyright (c) Roni Schuetz - All Rights Reserved
// * --------------------------------------------------------------------- *
// *                              Roni Schuetz                             *
// *              Copyright (c) 2008 All Rights reserved                   *
// *                                                                       *
// * Shared Cache high-performance, distributed caching and    *
// * replicated caching system, generic in nature, but intended to         *
// * speeding up dynamic web and / or win applications by alleviating      *
// * database load.                                                        *
// *                                                                       *
// * This Software is written by Roni Schuetz (schuetz AT gmail DOT com)   *
// *                                                                       *
// * This library is free software; you can redistribute it and/or         *
// * modify it under the terms of the GNU Lesser General Public License    *
// * as published by the Free Software Foundation; either version 2.1      *
// * of the License, or (at your option) any later version.                *
// *                                                                       *
// * This library is distributed in the hope that it will be useful,       *
// * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU      *
// * Lesser General Public License for more details.                       *
// *                                                                       *
// * You should have received a copy of the GNU Lesser General Public      *
// * License along with this library; if not, write to the Free            *
// * Software Foundation, Inc., 59 Temple Place, Suite 330,                *
// * Boston, MA 02111-1307 USA                                             *
// *                                                                       *
// *       THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.        *
// * --------------------------------------------------------------------- *
#endregion 

// *************************************************************************
//
// Name:      Country.cs
// 
// Created:   01-01-2008 SharedCache.com, rschuetz
// Modified:  01-01-2008 SharedCache.com, rschuetz : Creation
// Modified:  01-01-2008 SharedCache.com, rschuetz : added PrintToConsole()
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharedCache.WinServiceTestClient.Common
{
	/// <summary>
	/// A Region Xml Element
	/// <remarks>
	///		<ConCountry>
	///			<nCountryId>1</nCountryId>
	///			<cName>Afghanistan</cName>
	///			<cIso2>AF</cIso2>
	///			<cIso3>AFG</cIso3>
	///			<cCapitalCity>Kabul </cCapitalCity>
	///			<cMapReference>Asia </cMapReference>
	///			<cCurrency>Afghani </cCurrency>
	///			<cNameAr>افغانستان</cNameAr>
	///			<cNameZh>阿富汗</cNameZh>
	///			<cNameJa>アフガニスタン</cNameJa>
	///		</ConCountry>
	/// </remarks>
	/// </summary>
	[Serializable]
	public class Country
	{
		#region Property: Region
		private List<Region> region;
		
		/// <summary>
		/// Gets/sets the Region
		/// </summary>
		public List<Region> Region
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.region;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.region = value;  }
		}
		#endregion

		#region Property: CountryId
		private int countryId;
		
		/// <summary>
		/// Gets/sets the CountryId
		/// </summary>
		public int CountryId
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.countryId;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.countryId = value;  }
		}
		#endregion
		#region Property: Name
		private string name;
		
		/// <summary>
		/// Gets/sets the Name
		/// </summary>
		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.name;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.name = value;  }
		}
		#endregion
		#region Property: Iso2
		private string iso2;
		
		/// <summary>
		/// Gets/sets the Iso2
		/// </summary>
		public string Iso2
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.iso2;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.iso2 = value;  }
		}
		#endregion
		#region Property: Iso3
		private string iso3;
		
		/// <summary>
		/// Gets/sets the Iso3
		/// </summary>
		public string Iso3
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.iso3;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.iso3 = value;  }
		}
		#endregion
		#region Property: CapitalCityName
		private string capitalCityName;
		
		/// <summary>
		/// Gets/sets the CapitalCityName
		/// </summary>
		public string CapitalCityName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.capitalCityName;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.capitalCityName = value;  }
		}
		#endregion
		#region Property: MapReference
		private string mapReference;
		
		/// <summary>
		/// Gets/sets the MapReference
		/// </summary>
		public string MapReference
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.mapReference;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.mapReference = value;  }
		}
		#endregion
		#region Property: CurrencyName
		private string currencyName;
		
		/// <summary>
		/// Gets/sets the CurrencyName
		/// </summary>
		public string CurrencyName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.currencyName;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.currencyName = value;  }
		}
		#endregion

		#region Override Methods
		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Entering method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine);

			#region Override ToString() default with reflection
			Type t = this.GetType();
			PropertyInfo[] pis = t.GetProperties();
			for (int i = 0; i < pis.Length; i++)
			{
				try
				{
					PropertyInfo pi = (PropertyInfo)pis.GetValue(i);
					Console.WriteLine(
							string.Format(
							"{0}: {1}",
							pi.Name,
							pi.GetValue(this, new object[] { })
						)
					);
					sb.AppendFormat("{0}: {1}" + Environment.NewLine, pi.Name, pi.GetValue(this, new object[] { }));
				}
				catch (Exception ex)
				{
					Console.WriteLine("Could not log property. Ex. Message: " + ex.Message);
				}
			}
			#endregion Override ToString() default with reflection

			return sb.ToString();
		}

		#endregion Override Methods

		/// <summary>
		/// Prints to console.
		/// </summary>
		public void PrintToConsole()
		{
			Console.WriteLine(@"Country Id: {0} - Country Name: {1} [ISO2 Code: {2}]", this.CountryId, this.CurrencyName, this.Iso2);
		}
	}
}
