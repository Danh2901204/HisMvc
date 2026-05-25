const jwt = require('jsonwebtoken');
const env = require('../config/env');
const User = require('../modules/auth/models/user.model');

const setUser = async (req, res, next) => {
  let token;
  res.locals.user = null; // Initialize user as null

  if (req.cookies.token) {
    try {
      token = req.cookies.token;
      const decoded = jwt.verify(token, env.jwt.secret);
      const user = await User.findById(decoded.id).select('-password');
      if (user) {
        req.user = user;
        res.locals.user = user; // Make user available in templates
      }
    } catch (error) {
      // Invalid token, user remains null
      req.user = null;
    }
  }
  next();
};

module.exports = { setUser };