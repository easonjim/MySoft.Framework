﻿using System;
using System.Collections.Generic;

namespace MySoft.Net.Sockets
{
    public static class BinaryUtil
    {
        public static int IndexOf<T>(this IList<T> source, T target, int pos, int length)
        {
            for (int i = pos; i < pos + length; i++)
            {
                if (source[i].Equals(target))
                    return i;
            }

            return -1;
        }

        public static int? SearchMark<T>(this IList<T> source, T[] mark)
        {
            return SearchMark(source, 0, source.Count, mark);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark)
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = 0;

            while (true)
            {
                pos = source.IndexOf(mark[0], pos, length - pos + offset);

                if (pos < 0)
                    return null;

                matchCount = 1;

                for (int i = 1; i < mark.Length; i++)
                {
                    int checkPos = pos + i;

                    if (checkPos > endOffset)
                    {
                        //found end, return matched chars count
                        return (0 - i);
                    }

                    if (!source[checkPos].Equals(mark[i]))
                        break;

                    matchCount++;
                }

                if (matchCount == mark.Length)
                    return pos;

                //Reset next round read pos
                pos += matchCount;
                //clear matched chars count
                matchCount = 0;
            }
        }

        public static int StartsWith<T>(this IList<T> source, T[] mark)
        {
            return source.StartsWith(0, source.Count, mark);
        }

        public static int StartsWith<T>(this IList<T> source, int offset, int length, T[] mark)
        {
            int pos = offset;
            int endOffset = offset + length - 1;

            for (int i = 0; i < mark.Length; i++)
            {
                int checkPos = pos + i;

                if (checkPos > endOffset)
                    return i;

                if (!source[checkPos].Equals(mark[i]))
                    return -1;
            }

            return mark.Length;
        }

        public static bool EndsWith<T>(this IList<T> source, T[] mark)
        {
            return source.EndsWith(0, source.Count, mark);
        }

        public static bool EndsWith<T>(this IList<T> source, int offset, int length, T[] mark)
        {
            if (mark.Length > length)
                return false;

            for (int i = 0; i < Math.Min(length, mark.Length); i++)
            {
                if (!mark[i].Equals(source[offset + length - mark.Length + i]))
                    return false;
            }

            return true;
        }

        public static T[] CloneRange<T>(this T[] source, int offset, int length)
        {
            if (source == null) return new T[0];

            T[] target = new T[length];
            Array.Copy(source, offset, target, 0, length);
            return target;
        }
    }
}
