using nphw3listener;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var ip = IPAddress.Loopback;
var port = 27001;
var listener = new TcpListener(ip, port);

List<Car> cars = new List<Car>();

listener.Start();

while (true)
{
    var client = listener.AcceptTcpClient();
    var stream = client.GetStream();
    var br = new BinaryReader(stream);
    var bw = new BinaryWriter(stream);
    while (true)
    {
        var input = br.ReadString();
        var command = JsonSerializer.Deserialize<Command>(input);
        Console.WriteLine(command.Text);
        if (command.Params != null)
        {
            command.Params.ForEach(Console.WriteLine);
        }
        switch (command.Text)
        {
            case Command.Get:
                {
                    bw.Write(GetCommand());
                    break;
                }
            case Command.Post:
                {
                    bw.Write(PostCommand(command));
                    break;
                }
            case Command.Put:
                {
                    bw.Write(PutCommand(command));
                    break;
                }
            case Command.Delete:
                {
                    bw.Write(DeleteCommand(command));
                    break;
                }
            default:
                break;
        }
    }
}

string GetCommand()
{
    var carsAsString = JsonSerializer
                        .Serialize(cars.Select(c => c.ToString()));
    return carsAsString;
}

string PostCommand(Command command)
{
    var new_car = new Car();
    if (int.TryParse(command.Params[2], out int year))
    {
        new_car.Year = year;
        new_car.Mark = command.Params[0];
        new_car.Model = command.Params[1];
        if (cars.FirstOrDefault(c => c.Mark == new_car.Mark && c.Model == new_car.Model && c.Year == new_car.Year) == null)
        {
            cars.Add(new_car);
            return "success";
        }
        else
        {
            return "error";
        }
    }
    else
    {
        return "error";
    }
}

string PutCommand(Command command)
{
    if (int.TryParse(command.Params[2], out int year) && int.TryParse(command.Params[5], out int new_year))
    {
        var req_car = cars.FirstOrDefault(c => c.Mark == command.Params[0] && c.Model == command.Params[1] && c.Year == year);
        if (req_car != null)
        {
            req_car.Mark = command.Params[3];
            req_car.Model = command.Params[4];
            req_car.Year = new_year;
            return "success";
        }
        else
        {
            return "error";
        }
    }
    else
    {
        return "error";
    }
}

string DeleteCommand(Command command)
{
    if (int.TryParse(command.Params[2], out int year))
    {
        var req_car = cars.FirstOrDefault(c => c.Mark == command.Params[0] && c.Model == command.Params[1] && c.Year == year);
        if (req_car != null)
        {
            cars.Remove(req_car);
            return "success";  
        }
        else
        {
            return "error"; 
        }
    }
    else
    {
        return "error";
    }
}