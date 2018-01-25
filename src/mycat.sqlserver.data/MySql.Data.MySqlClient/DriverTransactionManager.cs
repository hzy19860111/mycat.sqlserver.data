using System;
using System.Collections;
using System.Transactions;
namespace MySql.Data.MySqlClient
{
	internal class DriverTransactionManager
	{
		private static Hashtable driversInUse = new Hashtable();
		public static Driver GetDriverInTransaction(Transaction transaction)
		{
			object syncRoot = DriverTransactionManager.driversInUse.SyncRoot;
			Driver result;
			lock (syncRoot)
			{
				result = (Driver)DriverTransactionManager.driversInUse[transaction.GetHashCode()];
			}
			return result;
		}
		public static void SetDriverInTransaction(Driver driver)
		{
			object syncRoot = DriverTransactionManager.driversInUse.SyncRoot;
			lock (syncRoot)
			{
				DriverTransactionManager.driversInUse[driver.CurrentTransaction.BaseTransaction.GetHashCode()] = driver;
			}
		}
		public static void RemoveDriverInTransaction(Transaction transaction)
		{
			object syncRoot = DriverTransactionManager.driversInUse.SyncRoot;
			lock (syncRoot)
			{
				DriverTransactionManager.driversInUse.Remove(transaction.GetHashCode());
			}
		}
	}
}
