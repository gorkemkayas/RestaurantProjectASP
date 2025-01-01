using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace deneme.Pages
{
    public class StaffDeleteModel : PageModel
    {
        private string? _connectionString;

        public StaffDeleteModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");
        }

        [BindProperty]
        public int staffId { get; set; }

        [BindProperty]
        public string staffName { get; set; }
        public void OnGet(int id, string Name)
        {
            staffId = id;
            staffName = Name;
        }

        public IActionResult OnPost(int staffId)
        {
            try
            {
                //silinmek istenen role un id sine göre orders tablosunda roleId ler null a atanacak.
                SetNullStaffId(staffId);
                DeleteStaffToDatabase(staffId);
                return RedirectToPage("/Staffs");

            }
            catch (InvalidOperationException ex)
            {
                ViewData["ConflictMessage"] = "The waiter <b>has an active order record</b>. To perform the deletion process, <b>you must wait for the order created by the relevant waiter to be completed.</b>";
                return Page();
            }
            catch (Exception)
            {

                ModelState.AddModelError(string.Empty, "DeleteStaffToDatabase method error...");
                return Page();
            }
        }

        private void DeleteStaffToDatabase(int stuffId)
        {

            string query = "DELETE FROM dbo.STAFF WHERE StaffId = @StaffId";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffId", stuffId);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SetNullStaffId(int stuffId)
        {
            string checkQuery = "SELECT COUNT(*) FROM [dbo].[ORDER] WHERE StaffId = @StaffId AND OrderStatus = 'Pending'";
            string updateQuery = "UPDATE [dbo].[ORDER] SET StaffId = NULL WHERE StaffId = @StaffId AND OrderStatus = 'Completed'";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Veritabanında veri tutarsızlığını önlemek için transaction kullanıyoruz.
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // İlk olarak, koşulları sağlamayan bir kayıt var mı kontrol edelim
                    using (var checkCommand = new SqlCommand(checkQuery, connection, transaction))
                    {
                        checkCommand.Parameters.AddWithValue("@StaffId", stuffId);
                        int count = (int)checkCommand.ExecuteScalar(); // Sonucu alıyoruz

                        if (count > 0)
                        {
                            // Eğer `OrderStatus != 'Completed'` kayıtları varsa hata fırlat
                            throw new InvalidOperationException($"OrderStatus 'Completed' olmayan {count} kayıt var.");
                        }
                    }

                    // Eğer hata fırlatılmadıysa, update işlemini yap
                    using (var updateCommand = new SqlCommand(updateQuery, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@StaffId", stuffId);
                        updateCommand.ExecuteNonQuery(); // Update sorgusunu çalıştırıyoruz
                    }

                    // İşlemleri onayla (commit)
                    transaction.Commit();
                    Console.WriteLine("Transaction başarıyla tamamlandı.");
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemleri geri al (rollback)
                    transaction.Rollback();
                    Console.WriteLine($"Transaction başarısız oldu: {ex.Message}");
                    throw new InvalidOperationException("Garsonun aktif siparişi olduğu için silme işlemi gerçekleştirilemedi.",ex);
                }
            }
        }


    }
}
