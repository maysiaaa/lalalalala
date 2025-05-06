using System.Data;
using LALALALALLA.Exceptions;
using LALALALALLA.Models;
using LALALALALLA.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace LALALALALLA.Services;
public interface IDbService
{
    public Task<IEnumerable<Trip_CountryGetDTO>> GetTripDetailsAsync();
    public Task<IEnumerable<TripGetDTO>> GetClientTripDetailsAsync(int clientId);
    public Task<int> CreateClientAsync(ClientCreateDTO client);
    public Task RegisterClientAsync(int clientId, int tripId);

}

public class DbService(IConfiguration config) : IDbService
{
    
    private async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(config.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        return connection;
    }

    public async Task<IEnumerable<Trip_CountryGetDTO>> GetTripDetailsAsync()
    {
        await using var connection = await GetConnectionAsync();
        var sql = """
                  SELECT t.IdTrip, t.Name, t.Description, t.DateTo,t.DateFrom, t.MaxPeople, c.Name 
                  FROM Trip t 
                  INNER JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                  INNER JOIN Country c ON ct.IdCountry = c.IdCountry
                  ORDER BY t.IdTrip;
                  """;
        await using var command = new SqlCommand(sql, connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        
        List<Trip_CountryGetDTO> trips = new List<Trip_CountryGetDTO>();
        int currentId = -1;
        List<string> countryNames = new List<string>();
        while (await reader.ReadAsync())
        {
            int newId = reader.GetInt32(0);
            if (newId != currentId)
            {
                countryNames.Add(reader.GetString(6));

                var tripDet = new Trip_CountryGetDTO
                {
                    IdTrip = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateTo = reader.GetDateTime(3),
                    DateFrom = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    CountryNames = countryNames
                };
                trips.Add(tripDet);
                currentId = newId;
            }
            else
            {
                countryNames.Add(reader.GetString(6));
            }
        }
        
        return trips;
        
        
    }


    public async Task<IEnumerable<TripGetDTO>> GetClientTripDetailsAsync(int clientId)
    {
        await using var connection = await GetConnectionAsync();
        
        var clientCheckSql = """
                            select IdClient, FirstName 
                            from Client
                            where Id = @clientId;
                            """;

        await using var clientCheckCommand = new SqlCommand(clientCheckSql, connection);
        clientCheckCommand.Parameters.AddWithValue("@clientId", clientId);
        await using var clientCheckReader = await clientCheckCommand.ExecuteReaderAsync();

        if (!await clientCheckReader.ReadAsync())
        {
            throw new NotFoundException($"Client with id {clientId} does not exist");
        }

        var sql = """
                  SELECT t.IdTRip, t.Name, t.Description, t.DateTo,t.DateFrom t.MaxPeople, ct.RegisteredAt, ct.PaymentDate
                  FROM Trip t
                  INNER JOIN CLient_Trip ct ON t.IdTrip = ct.IdTrip
                  INNER JOIN Client c ON c.IdClient = c.IdClient
                  WHERE c.IdClient = @ClientId;
                  """;
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        List<TripGetDTO> trips = new List<TripGetDTO>();

        while (await reader.ReadAsync())
        {
            var trip = new TripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateTo = reader.GetDateTime(3),
                DateFrom = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = reader.GetInt32(6),
                PaymentDate = reader.GetDateTime(7)

            };
            trips.Add(trip);

        }
        
        return trips;

    }

    public async Task<int> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = await GetConnectionAsync();

            var createClientSql = """
                                  insert into client
                                  output inserted.Id
                                  values (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                                  """;

            await using var createClientCommand =
                new SqlCommand(createClientSql, connection);
            createClientCommand.Parameters.AddWithValue("@FirstName", client.FirstName);
            createClientCommand.Parameters.AddWithValue("@LastName", client.LastName);
            createClientCommand.Parameters.AddWithValue("@Email", client.Email);
            createClientCommand.Parameters.AddWithValue("@Telephone", client.Telephone);
            createClientCommand.Parameters.AddWithValue("@Pesel", client.Pesel);



            var createdClientId = Convert.ToInt32(await createClientCommand.ExecuteScalarAsync());

            return createdClientId;

    }

    public async Task RegisterClientAsync(int clientId, int tripId)
    {
        await using var connection = await GetConnectionAsync(); 
        
        var clientCheckSql = """
                             select IdClient, FirstName 
                             from Client
                             where Id = @clientId;
                             """;

        await using var clientCheckCommand = new SqlCommand(clientCheckSql, connection);
        clientCheckCommand.Parameters.AddWithValue("@clientId", clientId);
        await using var clientCheckReader = await clientCheckCommand.ExecuteReaderAsync();

        if (!await clientCheckReader.ReadAsync())
        {
            throw new NotFoundException($"Client with id {clientId} does not exist");
        }
        
        var tripCheckSql = """
                             select IdTrip, Name 
                             from Trip
                             where Id = @tripId;
                             """;

        await using var tripCheckCommand = new SqlCommand(tripCheckSql, connection);
        clientCheckCommand.Parameters.AddWithValue("@tripId", tripId);
        await using var tripCheckReader = await tripCheckCommand.ExecuteReaderAsync();

        if (!await tripCheckReader.ReadAsync())
        {
            throw new NotFoundException($"Trip with id {clientId} does not exist");
        }

        var sql = """
                  insert into client_trip
                  values (@IdClient, @IdTrip, @RegisteredAt, null);
                  """;
        
        
        await using var createClientTripCommand = new SqlCommand(sql, connection);
        createClientTripCommand.Parameters.AddWithValue("@FirstName", clientId);
        createClientTripCommand.Parameters.AddWithValue("@LastName", tripId);
        createClientTripCommand.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.Year+DateTime.Now.Month+DateTime.Now.Day);
        //createClientTripCommand.Parameters.AddWithValue("@PaymentDate", null);

    }
}