const authService = require('./auth.service');

const renderLoginPage = (req, res) => {
  res.render('layouts/main', {
    body: 'pages/auth/login',
    title: 'Login',
    error: null
  });
};

const loginUser = async (req, res) => {
  try {
    const { username, password } = req.body;
    const { user, token } = await authService.login(username, password);

    res.cookie('token', token, {
      httpOnly: true,
      // secure: env.nodeEnv === 'production', // Use secure cookies in production
    });

    res.redirect('/dashboard'); // Redirect to a dashboard page after login
  } catch (error) {
    res.status(401).render('layouts/main', {
        body: 'pages/auth/login',
        title: 'Login',
        error: error.message
    });
  }
};

const logoutUser = (req, res) => {
    res.cookie('token', '', {
        httpOnly: true,
        expires: new Date(0)
    });
    res.redirect('/auth/login');
};


module.exports = {
  renderLoginPage,
  loginUser,
  logoutUser,
};