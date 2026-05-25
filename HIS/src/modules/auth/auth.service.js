const User = require('./models/user.model');
const jwt = require('jsonwebtoken');
const env = require('../../config/env');

const login = async (username, password) => {
  const user = await User.findOne({ username });
  if (!user) {
    throw new Error('Invalid credentials');
  }

  const isMatch = await user.comparePassword(password);
  if (!isMatch) {
    throw new Error('Invalid credentials');
  }

  const token = jwt.sign({ id: user._id, role: user.role }, env.jwt.secret, {
    expiresIn: env.jwt.expiresIn,
  });

  return { user, token };
};

module.exports = {
  login,
};