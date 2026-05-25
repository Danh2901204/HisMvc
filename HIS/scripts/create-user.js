// scripts/create-user.js
const mongoose = require('mongoose');
const dotenv = require('dotenv');
const path = require('path');

// Load env vars
dotenv.config({ path: path.join(__dirname, '../.env') });

const User = require('../src/modules/auth/models/user.model');
const { USER_ROLES } = require('../src/shared/enums');

const connectDB = async () => {
  try {
    await mongoose.connect(process.env.MONGODB_URI);
    console.log('MongoDB Connected...');
  } catch (err) {
    console.error(err.message);
    process.exit(1);
  }
};

const createUser = async () => {
  await connectDB();

  const args = process.argv.slice(2);
  if (args.length < 3) {
    console.error('Please provide username, password, and role.');
    console.log('Usage: node scripts/create-user.js <username> <password> <role>');
    process.exit(1);
  }

  const [username, password, role] = args;

  // Validate role
  const upperCaseRole = role.toUpperCase();
  if (!Object.values(USER_ROLES).map(r => r.toUpperCase()).includes(upperCaseRole)) {
      console.error(`Invalid role: ${role}`);
      console.log(`Available roles are: ${Object.values(USER_ROLES).join(', ')}`);
      process.exit(1);
  }

  try {
    // Check if user already exists
    const existingUser = await User.findOne({ username });
    if (existingUser) {
      console.log(`User "${username}" already exists.`);
      process.exit(0);
    }

    const user = new User({
      username,
      password,
      role: role.toLowerCase(),
    });

    await user.save();
    console.log(`User "${username}" created successfully with role "${user.role}"!`);
  } catch (error) {
    console.error('Error creating user:', error.message);
  } finally {
    mongoose.disconnect();
    console.log('MongoDB Disconnected.');
  }
};

createUser();