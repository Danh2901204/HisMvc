const jwt = require('jsonwebtoken');
const env = require('../config/env');
const User = require('../modules/auth/models/user.model');

const protect = async (req, res, next) => {
  let token;

  if (req.cookies.token) {
    try {
      // Get token from cookie
      token = req.cookies.token;

      // Verify token
      const decoded = jwt.verify(token, env.jwt.secret);

      // Get user from the token
      req.user = await User.findById(decoded.id).select('-password');

      if (!req.user) {
        return res.redirect('/auth/login');
      }

      next();
    } catch (error) {
      console.error(error);
      return res.redirect('/auth/login');
    }
  }

  if (!token) {
    return res.redirect('/auth/login');
  }
};

module.exports = { protect };