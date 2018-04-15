using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.APS.wmsSI.Helpers
{
    public static class AddressHelper
    {
        public static AddressBookType[] GetAddressTypes(params AddressBookType[] excludetypes)
        {
            var result = Enum.GetValues(typeof (AddressBookType)).Cast<AddressBookType>().ToArray();
            return excludetypes == null || excludetypes.Length == 0
                ? result
                : result.Where(p => !excludetypes.Contains(p)).ToArray();
        }

        /// <summary>
        /// Поиск адреса в коллекции.
        /// </summary>
        /// <param name="collection">Коллекция адресов</param>
        /// <param name="item">адрес для поиска</param>
        /// <returns>Если нашли - найденный адрес из коллекции, иначе - null</returns>
        public static AddressBook FindAddressInCollection(ICollection<AddressBook> collection, AddressBook item)
        {
            //Из коллекции выбираем не пустые адреса
            var notemptycol = collection.Where(p => !IsAddressEmpty(p)).ToArray();
            foreach (var adr in notemptycol)
            {
                var equals = true;
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKAPARTMENT) ||
                          adr.ADDRESSBOOKAPARTMENT.EqIgnoreCase(item.ADDRESSBOOKAPARTMENT);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKBUILDING) ||
                          adr.ADDRESSBOOKBUILDING.EqIgnoreCase(item.ADDRESSBOOKBUILDING);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKCITY) ||
                          adr.ADDRESSBOOKCITY.EqIgnoreCase(item.ADDRESSBOOKCITY);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKCOUNTRY) ||
                          adr.ADDRESSBOOKCOUNTRY.EqIgnoreCase(item.ADDRESSBOOKCOUNTRY);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKDISTRICT) ||
                          adr.ADDRESSBOOKDISTRICT.EqIgnoreCase(item.ADDRESSBOOKDISTRICT);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKREGION) ||
                          adr.ADDRESSBOOKREGION.EqIgnoreCase(item.ADDRESSBOOKREGION);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKSTREET) ||
                          adr.ADDRESSBOOKSTREET.EqIgnoreCase(item.ADDRESSBOOKSTREET);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKTYPECODE) ||
                          adr.ADDRESSBOOKTYPECODE.EqIgnoreCase(item.ADDRESSBOOKTYPECODE);
                equals &= string.IsNullOrEmpty(adr.ADDRESSBOOKRAW) ||
                          adr.ADDRESSBOOKRAW.EqIgnoreCase(item.ADDRESSBOOKRAW);
                equals &= adr.ADDRESSBOOKINDEX == null || adr.ADDRESSBOOKINDEX == item.ADDRESSBOOKINDEX;

                if (equals)
                    return adr;
            }
            return null;
        }

        public static decimal? FindAddressByRow(ICollection<AddressBook> collection, AddressBookWrapper item)
        {
            if (collection == null || collection.Count == 0)
                return null;

            AddressBook addressBook = null;

            if (!string.IsNullOrEmpty(item.ADDRESSBOOKRAW))
            {
                addressBook = collection.FirstOrDefault(ab => ab.ADDRESSBOOKRAW == item.ADDRESSBOOKRAW);
                if (addressBook != null)
                    return addressBook.ADDRESSBOOKID;
            }

            if (!string.IsNullOrEmpty(string.Format("{0}{1}{2}{3}{4}", item.ADDRESSBOOKAPARTMENT, item.ADDRESSBOOKBUILDING, item.ADDRESSBOOKCITY, item.ADDRESSBOOKREGION, item.ADDRESSBOOKSTREET)))
                addressBook = collection.FirstOrDefault(p =>
                    p.ADDRESSBOOKAPARTMENT.EqIgnoreCase(item.ADDRESSBOOKAPARTMENT) &&
                    p.ADDRESSBOOKBUILDING.EqIgnoreCase(item.ADDRESSBOOKBUILDING) &&
                    p.ADDRESSBOOKCITY.EqIgnoreCase(item.ADDRESSBOOKCITY) &&
                    p.ADDRESSBOOKREGION.EqIgnoreCase(item.ADDRESSBOOKREGION) &&
                    p.ADDRESSBOOKSTREET.EqIgnoreCase(item.ADDRESSBOOKSTREET));
            return addressBook != null ? addressBook.ADDRESSBOOKID : collection.Max(p => p.ADDRESSBOOKID);
        }

        public static AddressBookWrapper[] GetNotEmptyAddressBookByTypes(ICollection<AddressBookWrapper> collection, AddressBookType[] types)
        {
            if (collection == null)
                return null;

            if (types == null || types.Length == 0)
            {
                var res = collection.Where(p => !IsAddressEmpty(p)).ToArray();
                return res;
            }

            var result = new List<AddressBookWrapper>();
            foreach (var type in types)
            {
                var adrs = collection.Where(p => p.ADDRESSBOOKTYPECODE == type && !IsAddressEmpty(p)).ToArray();
                if (adrs.Length > 0)
                    result.AddRange(adrs);
            }

            return result.ToArray();
        }

        public static bool IsAddressEmpty(AddressBookWrapper address)
        {
            if (address == null)
                return true;

            var result = string.IsNullOrEmpty(address.ADDRESSBOOKINDEXSTR)
               && string.IsNullOrEmpty(address.ADDRESSBOOKCOUNTRY)
               && string.IsNullOrEmpty(address.ADDRESSBOOKREGION)
               && string.IsNullOrEmpty(address.ADDRESSBOOKCITY)
               && string.IsNullOrEmpty(address.ADDRESSBOOKDISTRICT)
               && string.IsNullOrEmpty(address.ADDRESSBOOKSTREET)
               && string.IsNullOrEmpty(address.ADDRESSBOOKBUILDING)
               && string.IsNullOrEmpty(address.ADDRESSBOOKAPARTMENT)
               && string.IsNullOrEmpty(address.ADDRESSBOOKRAW);
            return result;
        }

        public static bool IsAddressEmpty(AddressBook address)
        {
            if (address == null)
                return true;

            var result = !address.ADDRESSBOOKINDEX.HasValue 
                && string.IsNullOrEmpty(address.ADDRESSBOOKCOUNTRY)
                && string.IsNullOrEmpty(address.ADDRESSBOOKREGION)
                && string.IsNullOrEmpty(address.ADDRESSBOOKCITY)
                && string.IsNullOrEmpty(address.ADDRESSBOOKDISTRICT)
                && string.IsNullOrEmpty(address.ADDRESSBOOKSTREET)
                && string.IsNullOrEmpty(address.ADDRESSBOOKBUILDING)
                && string.IsNullOrEmpty(address.ADDRESSBOOKAPARTMENT)
                && string.IsNullOrEmpty(address.ADDRESSBOOKRAW);
            return result;
        }

        public static AddressBook GetAddressWithMaxIdByType(ICollection<AddressBook> collection, string typecode)
        {
            if (collection == null || string.IsNullOrEmpty(typecode))
                return null;

            var id = collection.Where(p => p.ADDRESSBOOKTYPECODE == typecode).ToArray()
                .Max(p => p.GetKey<decimal?>());
            if (!id.HasValue)
                return null;

            var result = collection.First(p => p.GetKey<decimal>() == id.Value);
            return result;
        }
    }
}
