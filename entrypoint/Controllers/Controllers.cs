using core;

namespace program;

public class Controllers(WebApplication app) {
    private readonly IConnectionFactory _connectionFactory = new ConnectionFactory();

    public void Register() {
        new AdminController().Register(app);
    }
}