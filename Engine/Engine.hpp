#ifndef __ENGINE_INIT_HPP__
#define __ENGINE_INIT_HPP__

#include <iostream>
#include <vector>
#include <string>


namespace Engine {
class Program {
    private:
        std::string m_name;
        std::string m_version;

    public:
        Program(std::string name="placeholder", std::string version) : m_name(name), m_version(version) {};
        ~Program();
};

}  // namespace Engine
#endif // __ENGINE_INIT_HPP__