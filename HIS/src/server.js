const app = require('./app');
const connectDB = require('./config/db');
const env = require('./config/env');

// Connect to database
connectDB();

const server = app.listen(env.port, () => {
  console.log(`Server is running on port ${env.port} in ${env.nodeEnv} mode.`);
});

process.on('unhandledRejection', (err) => {
  console.log('UNHANDLED REJECTION! 💥 Shutting down...');
  console.error(err.name, err.message);
  server.close(() => {
    process.exit(1);
  });
});