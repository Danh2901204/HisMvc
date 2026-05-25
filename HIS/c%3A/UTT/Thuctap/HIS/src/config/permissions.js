const allRoles = {
  admin: [
    'getUsers',
    'manageUsers',
    'getPatients',
    'managePatients',
    'getVisits',
    'manageVisits',
    'getInvoices',
    'manageInvoices',
    'getDepartments',
    'manageDepartments',
    'getRooms',
    'manageRooms',
    'getBeds',
    'manageBeds',
    'getDrugs',
    'manageDrugs',
  ],
  reception: [
    'getPatients',
    'managePatients',
    'getVisits',
    'manageVisits',
    'getInvoices',
    'manageInvoices',
  ],
  doctor: [
    'getPatients',
    'managePatients',
    'getVisits',
    'manageVisits',
    'getDrugs',
  ],
};

const roles = Object.keys(allRoles);
const roleRights = new Map(Object.entries(allRoles));

module.exports = {
  roles,
  roleRights,
};
const PERMISSIONS = {
  MANAGE_USERS: 'manage_users',
  MANAGE_DEPARTMENTS: 'manage_departments',
  MANAGE_ROOMS: 'manage_rooms',
  MANAGE_BEDS: 'manage_beds',
  MANAGE_DRUGS: 'manage_drugs',
  VIEW_REPORTS: 'view_reports',
  ACCESS_RECEPTION: 'access_reception',
  ACCESS_BILLING: 'access_billing',

  // Doctor-specific permissions
  VIEW_PATIENT_LIST: 'view_patient_list',
  VIEW_PATIENT_EMR: 'view_patient_emr',
  MANAGE_CLINICAL_NOTES: 'manage_clinical_notes',
  ORDER_LAB_TESTS: 'order_lab_tests',
  VIEW_LAB_RESULTS: 'view_lab_results',
  CREATE_PRESCRIPTION: 'create_prescription',
  MANAGE_TREATMENT_PLAN: 'manage_treatment_plan',
  MANAGE_INPATIENT_CARE: 'manage_inpatient_care',
  SIGN_DOCUMENTS: 'sign_documents',
  VIEW_OWN_STATS: 'view_own_stats',
};

const ROLES_PERMISSIONS = {
  [USER_ROLES.LEADER]: [PERMISSIONS.VIEW_REPORTS],
  [USER_ROLES.RECEPTIONIST]: [PERMISSIONS.ACCESS_RECEPTION],
  [USER_ROLES.DOCTOR]: [
    PERMISSIONS.VIEW_PATIENT_LIST,
    PERMISSIONS.VIEW_PATIENT_EMR,
    PERMISSIONS.MANAGE_CLINICAL_NOTES,
    PERMISSIONS.ORDER_LAB_TESTS,
    PERMISSIONS.VIEW_LAB_RESULTS,
    PERMISSIONS.CREATE_PRESCRIPTION,
    PERMISSIONS.MANAGE_TREATMENT_PLAN,
    PERMISSIONS.MANAGE_INPATIENT_CARE,
    PERMISSIONS.SIGN_DOCUMENTS,
    PERMISSIONS.VIEW_OWN_STATS,
  ],
  [USER_ROLES.NURSE]: [PERMISSIONS.VIEW_PATIENT_EMR],
  [USER_ROLES.LAB_TECHNICIAN]: [PERMISSIONS.ACCESS_LAB],
  [USER_ROLES.PHARMACIST]: [PERMISSIONS.ACCESS_PHARMACY],
};