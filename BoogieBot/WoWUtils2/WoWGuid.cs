using System;
using System.Collections.Generic;
using System.Text;

namespace Foole.WoW
{
    public class WoWGuid
    {
        public static byte BitCount1(byte x)
        {
            return ((byte)((x) & 1));
        }

        public static byte BitCount2(byte x) 
        {
            return (byte)(BitCount1(x) + BitCount1((byte)((x)>>1)));
        }

        public static byte BitCount4(byte x) 
        {
            return (byte)(BitCount2(x) + BitCount2((byte)((x) >> 2))); 
        }

        public static byte BitCount8(byte x) 
        {
            return (byte)(BitCount4(x) + BitCount4((byte)((x) >> 4)));
        }

        public WoWGuid() 
        {
            Clear();
        }

        public WoWGuid(UInt64 guid) 
        {
            Clear();
            Init(guid);
        }

        public WoWGuid(byte mask) 
        {
            Clear();
            Init(mask);
        }

        public WoWGuid(byte mask, byte[] fields) 
        {
            Clear();
            Init(mask, fields);
        }

        public void Free()
        {
            Clear();
        }

        public void Clear()
        {
            oldguid = 0;
            guidmask = 0;
            guidfields = null;
            fieldcount = 0;
        }

        public void Init(UInt64 guid)
        {
            Free();

            oldguid = guid;

            _CompileByOld();
        }

        public void Init(byte mask)
        {
            Free();

            guidmask = mask;

            if (guidmask == 0)
                _CompileByNew();
            else
                _AllocateFields();
        }

        public void Init(byte mask, byte[] fields)
        {
            Free();

            guidmask = mask;

            if (BitCount8(guidmask) == 0)
                return;

            _AllocateFields();
            
            for(int i = 0; i < BitCount8(guidmask); i++)     
                guidfields[i] = fields[i];
            
            fieldcount = BitCount8(guidmask);

            _CompileByNew();
        }

        public UInt64 GetOldGuid() {
            if (guidmask == 0)
                return 0;
            else
                return BitConverter.ToUInt64(GetNewGuid(), 0); 
        }

        public byte[] GetNewGuid() { return guidfields; }
        public byte GetNewGuidLen() { return BitCount8(guidmask); }
        public byte GetNewGuidMask() { return guidmask; }

        public void AppendField(byte field)
        {

            guidfields[fieldcount] = field;
            fieldcount++;

            if (fieldcount == BitCount8(guidmask))
                _CompileByNew();
        }

        private UInt64 oldguid;
        private byte guidmask;
        private byte[] guidfields;
        private byte fieldcount;

        private void _AllocateFields()
        {
            guidfields = new byte[8];
        }

        private void _CompileByOld()
        {

            for(int i = 0; i < 8; i++)
            {
                if ((char)(oldguid >> (56 - i*8)) != 0)
                {
                    guidmask |= (byte)(1 << (7 - i));
                }
            }
            
            if (BitCount8(guidmask) == 0)
                return;

            guidfields = new byte[8];//BitCount8(guidmask)];

            int j = 0;
            for(int i = 0; i < 8; i++)
            {
                if ((char)(oldguid >> (i * 8)) != 0)
                {
                    guidfields[j] = (byte)((char)(oldguid >> (i * 8)));
                    j++;
                }
                else
                {
                    guidfields[j] = 0x00;
                    j++;
                }
            }

            fieldcount = BitCount8(guidmask);
        }

        private void _CompileByNew()
        {
            int j = 0;
            for(int i = 0; i < 8; i++)
            {
                if ((guidmask & (1 << i)) == 1)
                {
                    UInt64 gfield = guidfields[j];
                    //oldguid |= (gfield << (8*i));
                    j++;
                }
            }
        }

        public static Boolean operator==(WoWGuid a, WoWGuid b)
        {
            return (a.GetOldGuid() == b.GetOldGuid());
        }


        public static Boolean operator !=(WoWGuid a, WoWGuid b)
        {
            return (a.GetOldGuid() != b.GetOldGuid());
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("GUID = {0}", GetOldGuid().ToString());
        }
    }
}
