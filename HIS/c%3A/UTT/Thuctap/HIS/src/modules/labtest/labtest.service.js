const LabTest = require('./labtest.model');

const getLabTests = async () => {
  return LabTest.find();
};

module.exports = {
  getLabTests,
};