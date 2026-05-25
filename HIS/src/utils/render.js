const renderWithLayout = (res, view, options) => {
  res.render('layouts/main', {
    body: view,
    ...options,
  });
};

module.exports = { renderWithLayout };