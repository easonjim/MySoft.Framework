﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// Table创建器
    /// </summary>
    /// <typeparam name="TCreator"></typeparam>
    [Serializable]
    public abstract class TableCreator<TCreator> : ITableCreator<TCreator>
        where TCreator : class
    {
        private Table table;

        /// <summary>
        /// 实例化TableCreator
        /// </summary>
        protected TableCreator()
        {
        }

        /// <summary>
        /// 实例化TableCreator
        /// </summary>
        /// <param name="tableName"></param>
        protected TableCreator(string tableName, string aliasName)
            : this()
        {
            this.table = new Table(tableName).As(aliasName);
        }

        /// <summary>
        /// 实例化DeleteCreator
        /// </summary>
        /// <param name="table"></param>
        protected TableCreator(Table table)
            : this()
        {
            this.table = table;
        }

        /// <summary>
        /// 返回table
        /// </summary>
        internal Table Table
        {
            get
            {
                return table;
            }
        }

        #region 设置表信息

        /// <summary>
        /// 设置表名
        /// </summary>
        /// <param name="tableName"></param>
        public TCreator From(string tableName)
        {
            this.table = new Table(tableName);
            return this as TCreator;
        }

        /// <summary>
        /// 设置表信息
        /// </summary>
        /// <param name="table"></param>
        public TCreator From(Table table)
        {
            this.table = table;
            return this as TCreator;
        }

        #endregion
    }

    /// <summary>
    /// Creator 基类
    /// </summary>
    [Serializable]
    public abstract class WhereCreator<TCreator> : TableCreator<TCreator>, IWhereCreator<TCreator>
        where TCreator : class
    {
        private IList<WhereClip> whereList;

        /// <summary>
        /// 实例化BaseCreator
        /// </summary>
        protected WhereCreator()
        {
            this.whereList = new List<WhereClip>();
        }

        /// <summary>
        /// 实例化BaseCreator
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="aliasName"></param>
        protected WhereCreator(string tableName, string aliasName)
            : base(tableName, aliasName)
        {
            this.whereList = new List<WhereClip>();
        }

        /// <summary>
        /// 实例化DeleteCreator
        /// </summary>
        /// <param name="table"></param>
        protected WhereCreator(Table table)
            : base(table)
        {
            this.whereList = new List<WhereClip>();
        }

        #region 内部属性

        /// <summary>
        /// 返回条件
        /// </summary>
        internal WhereClip Where
        {
            get
            {
                WhereClip newWhere = WhereClip.None;
                foreach (WhereClip where in whereList)
                {
                    newWhere &= where;
                }
                return newWhere;
            }
        }

        #endregion

        #region 增加一个条件

        /// <summary>
        /// 添加一个条件
        /// </summary>
        /// <param name="where"></param>
        public TCreator AddWhere(WhereClip where)
        {
            if (DataHelper.IsNullOrEmpty(where)) return this as TCreator;

            //不存在条件，则加入
            whereList.Add(where);

            return this as TCreator;
        }

        /// <summary>
        /// 添加一个条件
        /// </summary>
        /// <param name="where"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TCreator AddWhere(string where, params SQLParameter[] parameters)
        {
            if (string.IsNullOrEmpty(where)) return this as TCreator;

            return AddWhere(new WhereClip(where, parameters));
        }

        /// <summary>
        /// 添加一个条件
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public TCreator AddWhere(Field field, object value)
        {
            if (value == null)
                return AddWhere(field.IsNull());
            else
                return AddWhere(field == value);
        }

        /// <summary>
        /// 添加一个条件
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public TCreator AddWhere(string fieldName, object value)
        {
            return AddWhere(new Field(fieldName), value);
        }

        #endregion
    }
}
