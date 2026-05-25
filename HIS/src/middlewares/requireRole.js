const { ROLES_PERMISSIONS } = require('../config/permissions');

const requireRole = (requiredPermission) => {
  return (req, res, next) => {
    if (!req.user || !req.user.role) {
      return res.status(403).render('errors/403');
    }

    const userPermissions = ROLES_PERMISSIONS[req.user.role];

    if (userPermissions && userPermissions.includes(requiredPermission)) {
      return next();
    }

    return res.status(403).render('errors/403');
  };
};

module.exports = requireRole;